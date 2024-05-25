/*
* a kernel that add the elements of two vectors pairwise
*/
__kernel void apply_gaussian_blur(
	__global const int3 *Input,
	__global int3 *Output,
	__global const double *KernelMatrix,
	const int KernelSize)
{
	size_t x = get_global_id(0);
	size_t y = get_global_id(1);
	size_t width = get_global_size(0);
	size_t height = get_global_size(1);
	//if (x == 0)
	// printf{
	printf("%l, %l, %l, %l, %u\n", x, y, width, height, get_work_dim());
	//}
	

	double3 sum = 0;
	for (int i = 0; i < KernelSize; i++) {
		for (int j = 0; j < KernelSize; j++) {
			int kx = x - KernelSize / 2 + i;
			int ky = y - KernelSize / 2 + j;
			double kv = KernelMatrix[ky * KernelSize + kx];
			double3 kvMatrix = (kv, kv, kv);
			double3 input = convert_double3(Input[(x + kx) + (y + ky) * width]);
			sum += input * kvMatrix;
		}
	}

	Output[x] = Input[x]; // convert_int3(sum);
}