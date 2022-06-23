kernel void add (global const uchar4* A, 
          global const uchar4* B,
          global uchar4* result
) {
    int x = get_global_id(0);
    int y = get_global_id(1);    
    int i = y * get_global_size(0) + x;

    float4 fa = convert_float4(A[i]) / (float4)(255.0);
    float4 fb = convert_float4(B[i]) / (float4)(255.0);
    float4 fresult = (fa * fb) * (float4)(255.0);

    result[i] = convert_uchar4(fresult);
}