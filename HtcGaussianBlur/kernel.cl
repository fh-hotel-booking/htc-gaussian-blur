/*
* a kernel that add the elements of two vectors pairwise
*/
__kernel void apply_gaussian_blur(
	__global const double3 *Input,
	__global double3 *Output,
	__global const double *KernelMatrix,
	const int KernelSize)
{
	
	size_t x = get_global_id(0);
	size_t y = get_global_id(1);
	size_t width = get_global_size(0);
	size_t height = get_global_size(1);

	const double3 original_value = vload3(y + (height * x), (double*)Input);

	//if (x == 0)
	// printf{
	//printf("%i, %i, %i, %i, %u\n", x, y, width, height, get_work_dim());
	//}

	// printf("KernelSize: %d\n", KernelSize);
	
	double3 sum = 0;
	for (int i = 0; i < KernelSize-1; i++) {
		for (int j = 0; j < KernelSize-1; j++) {
			if(x == 0) {
				//TODO: clamping
				continue;
			}
			if(y == 0) {
				//TODO: clamping
				continue;
			}
			if(x == height-1) {
				//TODO: clamping
				continue;
			}
			if(y == width -1){
				//TODO: clamping
				continue;
			}
			int kx = x - KernelSize / 2 + i;
			int ky = y - KernelSize / 2 + j;

			//index: [(x + kx)* height + (y + ky) ]
			double3 input = original_value [(x + kx)* height + (y + ky) ];
			sum += input * KernelMatrix[ky * KernelSize + kx];
		}
	}
	
	vstore3(convert_int3(sum), y + (height * x), (int*)Output);

	// int id2 = y + (height * x);
	// if ((id2) == 2) {
	// 	printf("%d\n", id2);
	// 	printf("%d, %d, %d\n", Input[id2].x, Input[id2].y, Input[id2].z);
	// 	printf("%d, %d, %d\n", Output[id2].x, Output[id2].y, Output[id2].z);
	// }
	// j + (i * bitmap.Height)
	//Output[y + (height * x)] = (Input[y + (height * x)]); // convert_int3(sum);
	// if ((id2)  == 2) {
	// 	printf("%d\n", id2);
	// 	printf("%d, %d, %d\n", Input[id2].x, Input[id2].y, Input[id2].z);
	// 	printf("%d, %d, %d\n", Output[id2].x, Output[id2].y, Output[id2].z);
	// }
}