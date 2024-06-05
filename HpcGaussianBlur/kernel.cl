__kernel void apply_gaussian_blur(
	__global const int* Input,
	__global int* Output,
	__global const double* KernelMatrix,
	__local int* LocalInput,
	const int KernelSize,
	const int ExecuteColumnWise)
{
	
	size_t x = get_global_id(0);
	size_t y = get_global_id(1);
	size_t width = get_global_size(0);
	size_t height = get_global_size(1);

	size_t localSize = get_local_size(0);
	size_t localIndex = get_local_id(0);

	int index = (y * width + x) * 3;
	int idColor1 = index;
	int idColor2 = idColor1 + 1;
	int idColor3 = idColor1 + 2;

	int localIdColor1 = localIndex;
	int localIdColor2 = localIdColor1 + 1;
	int localIdColor3 = localIdColor1 + 2;

	double sumColor1 = 0;
	double sumColor2 = 0;
	double sumColor3 = 0;

	int halfKernelSize = KernelSize / 2;

	if (index == 0 || index == 2023) {
		printf("localSize: %u, localIndex: %u\n", localSize, localIndex);
	}

	LocalInput[localIdColor1] = Input[idColor1];
	LocalInput[localIdColor2] = Input[idColor2];
	LocalInput[localIdColor3] = Input[idColor3];
	barrier(CLK_LOCAL_MEM_FENCE);
	/*
	for (int dx = -halfKernelSize; dx <= halfKernelSize; dx++) {
			for (int dy = -halfKernelSize; dy <= halfKernelSize; dy++) {

				int neighborX = x + dx;
				int neighborY = y + dy;

				if (neighborX < 0) {
					neighborX = 0;
				}
				else if (neighborX >= width) {
					neighborX = width - 1;
				}

				if (neighborY < 0) {
					neighborY = 0;
				}
				else if (neighborY >= height) {
					neighborY = height - 1;
				}

				double kv = KernelMatrix[(dx + halfKernelSize) * KernelSize + (dy + halfKernelSize)];

				int neighborIndex = (neighborY * width + neighborX) * 3;
				sumColor1 += Input[neighborIndex] * kv;
				sumColor2 += Input[neighborIndex + 1] * kv;
				sumColor3 += Input[neighborIndex + 2] * kv;
			}
		}
	*/

	if (ExecuteColumnWise == 0) { // execute row wise
		
		for (int dx = -halfKernelSize; dx <= halfKernelSize; dx++) {
			int neighborX = x + dx;

			if (neighborX < 0) {
				neighborX = 0;
			}
			else if (neighborX >= width) {
				neighborX = width - 1;
			}

			double kv = KernelMatrix[dx + halfKernelSize];

			int neighborIndex = (y * width + neighborX) * 3;
			sumColor1 += Input[neighborIndex] * kv;
			sumColor2 += Input[neighborIndex + 1] * kv;
			sumColor3 += Input[neighborIndex + 2] * kv;
		}
	}
	else { // execute column wise
		for (int dy = -halfKernelSize; dy <= halfKernelSize; dy++) {
			int neighborY = y + dy;

			if (neighborY < 0) {
				neighborY = 0;
			}
			else if (neighborY >= height) {
				neighborY = height - 1;
			}

			double kv = KernelMatrix[dy + halfKernelSize];

			int neighborIndex = (neighborY * width + x) * 3;
			sumColor1 += Input[neighborIndex] * kv;
			sumColor2 += Input[neighborIndex + 1] * kv;
			sumColor3 += Input[neighborIndex + 2] * kv;
		}
	}
	
	Output[idColor1] = convert_int(sumColor1);
	Output[idColor2] = convert_int(sumColor2);
	Output[idColor3] = convert_int(sumColor3);
}
