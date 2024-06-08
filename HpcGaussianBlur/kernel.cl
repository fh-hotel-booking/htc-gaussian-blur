__kernel void apply_gaussian_blur(
	__global const int* Input,
	__global int* Output,
	__global const double* KernelMatrix,
	__local int* LocalInput,
	const int KernelSize,
	const int ExecuteRowWise)
{
	
	size_t x = get_global_id(0);
	size_t y = get_global_id(1);
	size_t width = get_global_size(0);
	size_t height = get_global_size(1);

	size_t localSize = 0;
	size_t localIndex = 0;
	size_t localIndex2 = 0;

	if (ExecuteRowWise == 0) {
		// width
		localSize = get_local_size(0);
		// 0 <-> width - 1
		localIndex = get_local_id(0);
	}
	else {
		// height
		localSize = get_local_size(1);
		// 0 <-> height - 1
		localIndex = get_local_id(1);
	}

	int index = (y * width + x) * 3;
	int idColor1 = index;
	int idColor2 = idColor1 + 1;
	int idColor3 = idColor1 + 2;

	int localIdColor1 = localIndex * 3;
	int localIdColor2 = localIdColor1 + 1;
	int localIdColor3 = localIdColor1 + 2;

	double sumColor1 = 0;
	double sumColor2 = 0;
	double sumColor3 = 0;

	int halfKernelSize = KernelSize / 2;

	LocalInput[localIdColor1] = Input[idColor1];
	LocalInput[localIdColor2] = Input[idColor2];
	LocalInput[localIdColor3] = Input[idColor3];
	barrier(CLK_LOCAL_MEM_FENCE);

	if (ExecuteRowWise == 0) { // execute row wise
		for (int dx = -halfKernelSize; dx <= halfKernelSize; dx++) {
			int neighborX = localIndex + dx;

			if (neighborX < 0) {
				neighborX = 0;
			}
			else if (neighborX >= width) {
				neighborX = width - 1;
			}

			double kv = KernelMatrix[dx + halfKernelSize];
			int neighborIndex = (neighborX) * 3;
			sumColor1 += LocalInput[neighborIndex] * kv;
			sumColor2 += LocalInput[neighborIndex + 1] * kv;
			sumColor3 += LocalInput[neighborIndex + 2] * kv;
		}
	}
	else { // execute column wise
		for (int dy = -halfKernelSize; dy <= halfKernelSize; dy++) {
			int neighborY = localIndex + dy;

			if (neighborY < 0) {
				neighborY = 0;
			}
			else if (neighborY >= height) {
				neighborY = height - 1;
			}

			double kv = KernelMatrix[dy + halfKernelSize];

			int neighborIndex = (neighborY) * 3;
			sumColor1 += LocalInput[neighborIndex] * kv;
			sumColor2 += LocalInput[neighborIndex + 1] * kv;
			sumColor3 += LocalInput[neighborIndex + 2] * kv;
		}
	}
	
	Output[idColor1] = convert_int(sumColor1);
	Output[idColor2] = convert_int(sumColor2);
	Output[idColor3] = convert_int(sumColor3);
}
