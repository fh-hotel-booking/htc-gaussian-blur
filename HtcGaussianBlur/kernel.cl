__kernel void apply_gaussian_blur(
	__global const int* Input,
	__global int* Output,
	__global const double* KernelMatrix,
	const int KernelSize)
{
	size_t x = get_global_id(0);
	size_t y = get_global_id(1);
	size_t width = get_global_size(0);
	size_t height = get_global_size(1);

	int index = (y * width + x) * 3;
	int idColor1 = index;
	int idColor2 = idColor1 + 1;
	int idColor3 = idColor1 + 2;

	double sumColor1 = 0;
	double sumColor2 = 0;
	double sumColor3 = 0;
	double sumKv = 0;

	int halfKernelSize = KernelSize / 2;

	for (int dx = -halfKernelSize; dx <= halfKernelSize; dx++) {
		for (int dy = -halfKernelSize; dy <= halfKernelSize; dy++) {

			int neighborX = x + dx;
			int neighborY = y + dy;

			if (neighborX < 0) neighborX = 0;
			else if (neighborX >= width) neighborX = width - 1;

			if (neighborY < 0) neighborY = 0;
			else if (neighborY >= height) neighborY = height - 1;

			double kv = KernelMatrix[(dx + halfKernelSize) * KernelSize + (dy + halfKernelSize)];

			int neighborIndex = (neighborY * width + neighborX) * 3;
			sumColor1 += Input[neighborIndex] * kv;
			sumColor2 += Input[neighborIndex + 1] * kv;
			sumColor3 += Input[neighborIndex + 2] * kv;
			sumKv += kv;
		}
	}

	Output[idColor1] = (int)(sumColor1 / sumKv);
	Output[idColor2] = (int)(sumColor2 / sumKv);
	Output[idColor3] = (int)(sumColor3 / sumKv);
}
/*
* 

__kernel void apply_gaussian_blur(
	__global const int *Input,
	__global int *Output,
	__global const double *KernelMatrix,
	const int KernelSize)
{
	size_t x = get_global_id(0);
	size_t y = get_global_id(1);
	size_t width = get_global_size(0);
	size_t height = get_global_size(1);
	
	int index = (y) + (height * x);
	int idColor1 = index * 3;
	int idColor2 = idColor1 + 1;
	int idColor3 = idColor1 + 2;


	double sumColor1 = 0;
	double sumColor2 = 0;
	double sumColor3 = 0;
	double sumKv = 0;
	for (int dx = 0; dx < KernelSize; dx++) {
		for (int dy = 0; dy < KernelSize; dy++) {
			int neighborX = x + (dx - ((KernelSize / 2)));
			int neighborY = y + (dy - ((KernelSize / 2)));

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
			double kv = KernelMatrix[(dy)+(KernelSize * dx)];
			if (index == 1200) {
				printf("index %d - x,y: %llu, %llu - kv %f, dx,dy - %d,%d neighborX,Y- %d,%d\n", index, x, y, kv, dx, dy, neighborX, neighborX);
			}
			sumColor1 += (Input[(neighborY) + (height * neighborX) * 3] * kv);
			sumColor2 += (Input[(neighborY) + (height * neighborX) * 3 + 1] * kv);
			sumColor3 += (Input[(neighborY) + (height * neighborX) * 3 + 2] * kv);
			sumKv += kv;
		}
	}

	Output[idColor1] = sumColor1 / sumKv;
	Output[idColor2] = sumColor2 / sumKv;
	Output[idColor3] = sumColor3 / sumKv;
}
*/