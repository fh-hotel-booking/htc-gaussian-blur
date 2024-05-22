/*
* a kernel that add the elements of two vectors pairwise
*/
__kernel void apply_gaussian_blur(
	__global const int *A,
	__global int *B,
	__global const double *C)
{
	size_t i = get_global_id(0);
	B[i] = A[i] * C[5];
}