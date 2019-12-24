MKDIR tmp
robocopy.exe IO_1\		tmp\ /XF "*.config" /MOV /E
RMDIR /S /Q tmp
MKDIR tmp
robocopy.exe IO_2\		tmp\ /XF "*.config" /MOV /E
RMDIR /S /Q tmp
MKDIR tmp
robocopy.exe JOIN_1\	tmp\ /XF "*.config" /MOV /E
RMDIR /S /Q tmp
MKDIR tmp
robocopy.exe JOIN_2\	tmp\ /XF "*.config" /MOV /E
RMDIR /S /Q tmp
MKDIR tmp
robocopy.exe JOIN_3\	tmp\ /XF "*.config" /MOV /E
RMDIR /S /Q tmp
MKDIR tmp
robocopy.exe JOIN_4\	tmp\ /XF "*.config" /MOV /E
RMDIR /S /Q tmp
MKDIR tmp
robocopy.exe MGM\		tmp\ /XF "*.config" /MOV /E
RMDIR /S /Q tmp
MKDIR tmp
robocopy.exe SORT\		tmp\ /XF "*.config" /MOV /E
RMDIR /S /Q tmp