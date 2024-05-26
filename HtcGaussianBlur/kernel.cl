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
	int idColor1 = index * 3;
	int idColor2 = idColor1 + 1;
	int idColor3 = idColor1 + 2;
	//if (x == 0)
	//{
	//printf("%i, %i, %i, %i, %u\n", x, y, width, height, get_work_dim());
	//}
	

	
	double sumColor1 = 0;
	double sumColor2 = 0;
	double sumColor3 = 0;
	for (int i = 0; i < KernelSize; i++) {
		for (int j = 0; j < KernelSize; j++) {
			int realx = x + i;
			if (realx < 0) {
				realx = 0;
			}
			else if (realx >= width) {
				realx = width - 1;
			}
			int realy = y + i;
			if (realy < 0) {
				realy = 0;
			}
			else if (realy >= height) {
				realy = height - 1;
			}

			int kx = x - KernelSize / 2 + i;
			int ky = y - KernelSize / 2 + j;
			double kv = KernelMatrix[i * KernelSize + j];
			sumColor1 += (Input[(realy)+(height * realx) * 3] * kv);
			sumColor2 += (Input[(realy)+(height * realx) * 3 + 1] * kv);
			sumColor3 += (Input[(realy)+(height * realx) * 3 + 2] * kv);
		}
	}
/*
	for (int i = -KERNELHALFWIDTH; i <= KERNELHALFWIDTH; i = i + 1) {
#if defined(CONVOLE_HORIZONTAL)
		int realx = x + i;
		if (realx < 0) {
			realx = 0;
		}
		else if (realx >= IMAGEWIDTH) {
			realx = IMAGEWIDTH - 1;
		}

		float kv = lpKernel[i + KERNELHALFWIDTH];
		v_r = v_r + lpImage[(realx + y * IMAGEHEIGHT) * 4 + 0] * kv;
		v_g = v_g + lpImage[(realx + y * IMAGEHEIGHT) * 4 + 1] * kv;
		v_b = v_b + lpImage[(realx + y * IMAGEHEIGHT) * 4 + 2] * kv;
#else
		int realy = y + i;
		if (realy < 0) {
			realy = 0;
		}
		else if (realy >= IMAGEHEIGHT) {
			realy = IMAGEHEIGHT - 1;
		}

		float kv = lpKernel[i + KERNELHALFWIDTH];
		v_r = v_r + lpImage[(x + realy * IMAGEHEIGHT) * 4 + 0] * kv;
		v_g = v_g + lpImage[(x + realy * IMAGEHEIGHT) * 4 + 1] * kv;
		v_b = v_b + lpImage[(x + realy * IMAGEHEIGHT) * 4 + 2] * kv;
#endif
	}
*/
	if (sumColor1 > 255) {
		sumColor1 = 255;
	}
	else if (sumColor1 < 0) {
		sumColor1 = 0;
	}
	if (sumColor2 > 255) {
		sumColor2 = 255;
	}
	else if (sumColor2 < 0) {
		sumColor2 = 0;
	}
	if (sumColor3 > 255) {
		sumColor3 = 255;
	}
	else if (sumColor3 < 0) {
		sumColor3 = 0;
	}

	Output[idColor1] = sumColor1;
	Output[idColor2] = sumColor2;
	Output[idColor3] = sumColor3;	
}