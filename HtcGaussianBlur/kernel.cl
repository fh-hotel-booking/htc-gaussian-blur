/*
* a kernel that add the elements of two vectors pairwise
*/
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
	int idColor1 = (y * 3) + (height * x * 3);
	int idColor2 = (y * 3) + (height * x * 3) + 1;
	int idColor3 = (y * 3) + (height * x * 3) + 2;
	//if (x == 0)
	//{
	//printf("%i, %i, %i, %i, %u\n", x, y, width, height, get_work_dim());
	//}
	
	
	double sumColor1 = 0;
	double sumColor2 = 0;
	double sumColor3 = 0;
	for (int i = 0; i < KernelSize; i++) {
		for (int j = 0; j < KernelSize; j++) {
			int kx = x - KernelSize / 2 + i;
			int ky = y - KernelSize / 2 + j;
			double kv = KernelMatrix[ky * KernelSize + kx];
			sumColor1 += (Input[idColor1] * kv);
			sumColor2 += (Input[idColor2] * kv);
			sumColor3 += (Input[idColor3] * kv);
		}
	}

	Output[idColor1] = sumColor1;
	Output[idColor2] = sumColor2;
	Output[idColor3] = sumColor3;

	//printf("id: %d - %d, %d, %d \n", id2, Input[id2].x, Input[id2].y, Input[id2].z);
	if (Input[idColor1] == 252 && Input[idColor2] == 255 && Input[idColor3] == 255) {
		printf("Input id: %d - %d, %d, %d \n", idColor1, Input[idColor1], Input[idColor2], Input[idColor3]);
	}
	
	if ((idColor1) > 290 && idColor1 < 302) {
		printf("Input id: %d - %d, %d, %d\n", idColor1, Input[idColor1], Input[idColor2], Input[idColor3]);
		printf("Output id: %d - %d, %d, %d\n", idColor1, Output[idColor1], Output[idColor2], Output[idColor3]);
	}
	// j + (i * bitmap.Height)
	 // convert_int3(sum);
	/*
	if ((id2) == 399) {
		printf("id: %d - %d, %d, %d\n", id2, Input[id2].x, Input[id2].y, Input[id2].z);
		printf("id: %d - %d, %d, %d\n", id2, Output[id2].x, Output[id2].y, Output[id2].z);
	}
	*/
	
}