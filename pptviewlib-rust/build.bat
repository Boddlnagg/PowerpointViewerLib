REM Note that libgcc_s_dw2-1.dll is linked statically on Win64, but dynamically on Win32, so it needs to be in the PATH on 32-bit
del target\pptviewlib-*.dll
cargo build
xcopy /Y target\pptviewlib-*.dll pptviewlib.dll
copy pptviewlib.dll ..\bin\Debug\