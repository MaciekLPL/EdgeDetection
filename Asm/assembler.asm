;* ******************************************
;* Temat: Wykrywanie krawêdzi - Operator Sobela
;* Autor: Maciej Lejczak, Informatyka Katowice, semestr 5, grupa 2
;* Prowadz¹dzy: mgr in¿. Krzysztof Hanzel
;* Rok akademicki: 2021/2022
;* ******************************************/
.code

mainSobel proc

    columns equ 0
    rows equ 8
    output equ 16
    input equ 24
    bytes_per_r equ 32
    
    ;rcx - input
    ;rdx - output
    ;r8 - rows
    ;r9 - cols

    ;----INITIALIZATION----

    PUSH    rbp                         
    MOV     rbp, rsp                    
    SUB     rsp, 40                     ;make space for local variables


    MOV     [rsp + input], rcx          ;load local variables
    MOV     [rsp + output], rdx
    MOV     [rsp + rows], r8
    MOV     [rsp + columns], r9

    IMUL    r9, 4                       ;bytes_per_row = columns * 4 bpp
    MOV     [rsp + bytes_per_r], r9


    MOV     r8, [rsp + columns]         ;offset - skip first and last column
    SUB     r8, 2


    PXOR XMM5, XMM5                     ;prepare xmm register for further use
    MOV R11d, 0FFFFFFh 
    MOVD xmm5, R11d                     ;xmm5 register will hold three 255 values
    PMOVZXBD xmm5, xmm5                 ;and will be used to compare with calculated values

    IMUL r9, [rsp + rows]
    ADD rcx, r9
    ADD rdx, r9

    MOV rax, 1                          ;rbx - column counter from 1 to columns - 2

    ;----INNER LOOP (COLUMNS)----
    columnLoop:
                                        
    MOV r9, rax                        ;current pixel in row = columnCounter * 4 bpp
    IMUL r9, 4
    MOV r10, r9

    ADD r9, rcx                        ;Address of current pixel in input
    ADD r10, rdx                        ;Address of current pixel in output


    ;rax - row counter
    ;rbx - column counter
    ;r9 - pointer to current pixel in INPUT     (rbx * 4 + rcx)
    ;r10 - pointer to current pixel in OUTPUT    (rbx * 4 + rdx)

    ;                       {-1   0   1}        { 1   2   1}
    ;   Kernels:        X:  {-2   0   2}    Y:  { 0   0   0}
    ;                       {-1   0   1}        {-1  -2  -1}

    PXOR xmm0, xmm0                     ;clear xmm registers
    PXOR xmm1, xmm1
    PXOR xmm2, xmm2
    PXOR xmm3, xmm3
    PXOR xmm4, xmm4

    ;xmm0 - values of RGBx
    ;xmm4 - values of RGBy
    ;xmm1 - left pixel of currently computed row
    ;xmm2 - middle pixel of currently computed row
    ;xmm3 - right pixel of currently computed row

    ;----MIDDLE ROW CALCULATIONS----
    ;---- X kernel {-2,  0,  2} ----
    CALL get_row
    ; (X)
    PSUBD xmm0, xmm1                    ;-2 * left pixel    (X)
    PSUBD xmm0, xmm1
    PADDD xmm0, xmm3                    ;+2 * right pixel   (X)
    PADDD xmm0, xmm3


    ;----BOTTOM ROW CALCULATIONS----
    ;---- X kernel {-1,  0,  1} ----       
    ;---- Y kernel { 1,  2,  1} ----       
    ADD r9, [rsp + bytes_per_r]
    CALL get_row
    ; (X)
    PSUBD xmm0, xmm1                    ;-1 * left pixel
    PADDD xmm0, xmm3                    ;+1 * right pixel
    ; (Y)
    PADDD xmm4, xmm1                    ;+1 * left pixel
    PADDD xmm4, xmm2                    ;+2 * center pixel
    PADDD xmm4, xmm2                    
    PADDD xmm4, xmm3                    ;+1 * right pixel


    ;---- TOP ROW CALCULATIONS  ----
    ;---- X kernel {-1,  0,  1} ----       
    ;---- Y kernel {-1, -2, -1} ---- 
    SUB r9, [rsp + bytes_per_r]
    SUB r9, [rsp + bytes_per_r]
    CALL get_row
    ; (X)
    PSUBD xmm0, xmm1                    ;-1 * left pixel
    PADDD xmm0, xmm3                    ;+1 * right pixel
    ; (Y)
    PSUBD xmm4, xmm1                    ;-1 * left pixel
    PSUBD xmm4, xmm2                    ;-2 * center pixel
    PSUBD xmm4, xmm2
    PSUBD xmm4, xmm3                    ;-1 * right pixel


    ;---- SQRT ( (RGBx * RGBx) + (RGBy * RGBy) ) ----
    PMULLD xmm0, xmm0                   ;(RGBx * RGBx)
    PMULLD xmm4, xmm4                   ;(RGBy * RGBy)
    
    PADDD xmm0, xmm4                    ;(RGBx * RGBx) + (RGBy * RGBy)

    CVTDQ2PS xmm0, xmm0                 ;parse to floats
    SQRTPS xmm0, xmm0                   ;SQRT ( )
    CVTPS2DQ   xmm0, xmm0               ;parse back to ints
    

    ;---- MIN ( Rxy, 255 ), MIN ( Gxy, 255 ), MIN ( Bxy, 255 ) ----
    PMINUD xmm0, xmm5

    ;---- INSERT PIXEL INTO OUTPUT IMAGE ----
    PEXTRB byte ptr [r10], xmm0, 0
    PEXTRB byte ptr [r10 + 1], xmm0, 4
    PEXTRB byte ptr [r10 + 2], xmm0, 8


    INC rax                             ;increment column counter
    CMP rax, r8                         ;check if all columns have been processed
    JLE columnLoop                      ;jump if not all columns have been processed  (columnCounter <= columns-2)
    

    ;---- QUIT PROGRAM ----

    LEAVE
    RET

    ;---- GET 3 PIXELS IN ROW----
    get_row:
    MOVD xmm1, dword ptr [r9 - 4]       ;pixel on the left -> xmm1
    PMOVZXBD xmm1, xmm1

    MOVD xmm2, dword ptr [r9]           ;pixel in the center  -> xmm2
    PMOVZXBD xmm2, xmm2

    MOVD xmm3, dword ptr [r9 + 4]       ;pixel on the right -> xmm3
    PMOVZXBD xmm3, xmm3
    RET

mainSobel endp

END