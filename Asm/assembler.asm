
.code

;rcx - input
;rdx - output
;r8 - rows
;r9 - cols

mainSobel proc

columns equ 0
rows equ 8
output equ 16
input equ 24
bytes_per_r equ 32


    push    rbp
    mov     rbp, rsp
    sub     rsp, 40

    push rbx

    cmp     r8, 3             
    jl      too_small
    cmp     r9, 3               
    jl      too_small


    mov     [rsp + input], rcx
    mov     [rsp + output], rdx
    mov     [rsp + rows], r8
    mov     [rsp + columns], r9

    imul    r9, 4
    mov     [rsp + bytes_per_r], r9

    mov     rax, [rsp + rows]           ;Offset - skip first and last row
    sub     rax, 2

    mov     r8, [rsp + columns]         ;Offset - skip first and last column
    sub     r8, 2

    add rcx, [rsp + bytes_per_r]        ;skip first row
    add rdx, [rsp + bytes_per_r]        ;skip first row 

    rowLoop:
    mov     rbx, 1      ;column counter [1 , r9 - 2]

    columnLoop:
    
    ;                  -1   0   1             -1   -2   -1
    ; Kernels:      X  -2   0   2          Y   0    0    0
    ;                  -1   0   1              1    2    1

    mov r9, rbx        ;address of pixel to R9
    imul r9, 4
    mov r10, r9

    add r9, rcx
    add r10, rdx

    pxor xmm0, xmm0
    pxor xmm1, xmm1
    pxor xmm2, xmm2
    pxor xmm3, xmm3


    ;middle row
    CALL get_pixel
    PADDD xmm0, xmm1    ;X kernel (+2, -2)
    PADDD xmm0, xmm1
    PSUBD xmm0, xmm2
    PSUBD xmm0, xmm2


    ; bottom row
    add r9, [rsp + bytes_per_r]
    CALL get_pixel
    
    PADDD xmm0, xmm1    ;X kernel (+1, -1)
    PSUBD xmm0, xmm2
    PADDD xmm3, xmm1    ;Y kernel (+1, +1)
    PADDD xmm3, xmm2

    PINSRB xmm1, byte ptr [r9 + 2], 0      ;Y kernel (+2)
    PINSRB xmm1, byte ptr [r9 + 1], 4
    PINSRB xmm1, byte ptr [r9], 8

    PADDD xmm3, xmm1
    PADDD xmm3, xmm1


    ; top row
    sub r9, [rsp + bytes_per_r]
    sub r9, [rsp + bytes_per_r]
    CALL get_pixel

    PADDD xmm0, xmm1    ;X kernel (+1, -1)
    PSUBD xmm0, xmm2
    PSUBD xmm3, xmm1    ;Y kernel (-1, -1)
    PSUBD xmm3, xmm2

    PINSRB xmm1, byte ptr [r9 + 2], 0      ;Y kernel (-2)
    PINSRB xmm1, byte ptr [r9 + 1], 4
    PINSRB xmm1, byte ptr [r9], 8
    PSUBD xmm3, xmm1
    PSUBD xmm3, xmm1
    

    PMULLD xmm0, xmm0
    PMULLD xmm3, xmm3
    
    PADDD xmm0, xmm3

    CVTDQ2PS xmm0, xmm0
    SQRTPS xmm0, xmm0

    CVTPS2DQ   xmm0, xmm0

    MOV R11d, 255

    PINSRB xmm1, R11d, 0
    PINSRB xmm1, R11d, 4
    PINSRB xmm1, R11d, 8

    PMINUD xmm0, xmm1
    PEXTRB byte ptr [r10], xmm0, 0
    PEXTRB byte ptr [r10 + 1], xmm0, 4
    PEXTRB byte ptr [r10 + 2], xmm0, 8


    add rbx, 1
    cmp rbx, r8
    jle columnLoop
    

    add rcx, [rsp + bytes_per_r] 
    add rdx, [rsp + bytes_per_r] 
    sub rax, 1
    cmp rax, 0
    jg rowLoop

    too_small:

    pop rbx
    leave
    ret


    get_pixel:
    PINSRB xmm1, byte ptr [r9 + 6], 0
    PINSRB xmm1, byte ptr [r9 + 5], 4
    PINSRB xmm1, byte ptr [r9 + 4], 8

    PINSRB xmm2, byte ptr [r9 - 2], 0
    PINSRB xmm2, byte ptr [r9 - 3], 4
    PINSRB xmm2, byte ptr [r9 - 4], 8
    ret

mainSobel endp

END