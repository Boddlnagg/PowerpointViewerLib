#![allow(non_snake_case)]
#![feature(unsafe_destructor)]
extern crate winapi;

use winapi::{DWORD,HANDLE,LPVOID,LPCVOID,BOOL,LPCWSTR,LPSECURITY_ATTRIBUTES,SIZE_T,FALSE};
use std::slice::{from_raw_buf, from_raw_mut_buf};
use std::io::IoResult;
use std::io::IoError;
use std::mem;

pub const PAGE_READWRITE: DWORD = 0x04;
pub const STANDARD_RIGHTS_REQUIRED: DWORD = 0x000F0000;
pub const SECTION_QUERY: DWORD = 0x0001;
pub const SECTION_MAP_WRITE: DWORD = 0x0002;
pub const SECTION_MAP_READ: DWORD = 0x0004;
pub const SECTION_MAP_EXECUTE: DWORD = 0x0008;
pub const SECTION_EXTEND_SIZE: DWORD = 0x0010;
pub const SECTION_ALL_ACCESS: DWORD = (STANDARD_RIGHTS_REQUIRED|SECTION_QUERY|
    SECTION_MAP_WRITE |      
    SECTION_MAP_READ |      
    SECTION_MAP_EXECUTE |    
    SECTION_EXTEND_SIZE);
pub const FILE_MAP_ALL_ACCESS: DWORD = SECTION_ALL_ACCESS;

#[link(name = "user32")]
extern "system" {    
    pub fn CreateFileMappingW(
        hFile: HANDLE,
        lpAttributes: LPSECURITY_ATTRIBUTES,
        flProtect: DWORD,
        dwMaximumSizeHigh: DWORD,
        dwMaximumSizeLow: DWORD,
        lpName: LPCWSTR
    ) -> HANDLE;
    
    pub fn OpenFileMappingW(
        dwDesiredAccess: DWORD,
        bInheritHandle: BOOL,
        lpName: LPCWSTR
    ) -> HANDLE;
}
#[link(name = "kernel32")]
extern "system" {
    pub fn MapViewOfFile(
        hFileMappingObject: HANDLE,
        dwDesiredAccess: DWORD,
        dwFileOffsetHighD: DWORD,
        dwFileOffsetLowD: DWORD,
        dwNumberOfBytesToMap: SIZE_T
    ) -> LPVOID;
    
    pub fn UnmapViewOfFile(
        lpBaseAddress: LPCVOID
    ) -> BOOL;
}

pub struct FileMapping {
    handle: HANDLE,
    view: LPVOID,
    size: u32
}

pub struct StructFileMapping<T> {
    handle: HANDLE,
    view: LPVOID,
}

impl FileMapping {
    pub fn create(name: &str, size: u32) -> IoResult<FileMapping> {
        let mut name = name.as_slice().utf16_units().collect::<Vec<u16>>();
        name.push(0);
        let hMapFile = unsafe {
            CreateFileMappingW(
                winapi::INVALID_HANDLE_VALUE,    // use paging file
                0 as winapi::LPSECURITY_ATTRIBUTES,                    // default security
                PAGE_READWRITE,          // read/write access
                0,                       // maximum object size (high-order DWORD)
                size,                // maximum object size (low-order DWORD)
                name.as_ptr()/* as (*const i8)*/
            ) };
            
        if hMapFile.is_null() {
            return Err(IoError::last_error());
        }
        
        unsafe { FileMapping::init_from_handle(size, hMapFile) }
    }
    
    // unsafe because the size must match the size when creating
    pub unsafe fn open(name: &str, size: u32) -> IoResult<FileMapping> {
        let mut name = name.as_slice().utf16_units().collect::<Vec<u16>>();
        name.push(0);
        let hMapFile = 
            OpenFileMappingW(
                FILE_MAP_ALL_ACCESS,   // read/write access
                FALSE,                 // do not inherit the name
                name.as_ptr()
            );
            
        if hMapFile.is_null() {
            return Err(IoError::last_error());
        }
        
        FileMapping::init_from_handle(size, hMapFile)
    }
    
    unsafe fn init_from_handle(size: u32, hMapFile: HANDLE) -> IoResult<FileMapping> {        
        let pBuf = MapViewOfFile(hMapFile,   // handle to map object
                       FILE_MAP_ALL_ACCESS, // read/write permission
                       0,
                       0,
                       size
                   );

        if pBuf.is_null()
        {
            let err = IoError::last_error();
            winapi::CloseHandle(hMapFile);
            return Err(err);
        }
        
        Ok(FileMapping { handle: hMapFile, view: pBuf, size: size })
    }
    
    pub fn get_buffer<'a>(&'a self) -> &'a [u8] {
        let pBuf = self.view as *mut u8;
        unsafe { from_raw_buf(mem::transmute(&pBuf), self.size as uint) }
    }
    
    pub fn get_mut_buffer<'a>(&'a mut self) -> &'a mut [u8] {
        let pBuf = self.view as *mut u8;
        unsafe { from_raw_mut_buf(mem::transmute(&pBuf), self.size as uint) }
    }
}

impl Drop for FileMapping {
    fn drop(&mut self) {
        unsafe {
            UnmapViewOfFile(self.view);
            winapi::CloseHandle(self.handle);
        };
    }
}

impl<T> StructFileMapping<T> {
    pub fn create(name: &str) -> IoResult<StructFileMapping<T>> {
        let mut name = name.as_slice().utf16_units().collect::<Vec<u16>>();
        name.push(0);
        let size = mem::size_of::<T>();
        assert!(size <= std::u32::MAX as uint);
        let size = size as u32;
        let hMapFile = unsafe {
            CreateFileMappingW(
                winapi::INVALID_HANDLE_VALUE,    // use paging file
                0 as winapi::LPSECURITY_ATTRIBUTES,                    // default security
                PAGE_READWRITE,          // read/write access
                0,                       // maximum object size (high-order DWORD)
                size,                // maximum object size (low-order DWORD)
                name.as_ptr() /* as (*const i8)*/
            ) };
            
        if hMapFile.is_null() {
            return Err(IoError::last_error());
        }
        
        unsafe { StructFileMapping::init_from_handle(size, hMapFile) }
    }
    
    // unsafe because the mapping must have been created for the same type
    pub unsafe fn open(name: &str) -> IoResult<StructFileMapping<T>> {
        let mut name = name.as_slice().utf16_units().collect::<Vec<u16>>();
        name.push(0);
        let size = mem::size_of::<T>();
        assert!(size <= std::u32::MAX as uint);
        let size = size as u32;
        let hMapFile = 
            OpenFileMappingW(
                FILE_MAP_ALL_ACCESS,   // read/write access
                FALSE,                 // do not inherit the name
                name.as_ptr()
            );
            
        if hMapFile.is_null() {
            return Err(IoError::last_error());
        }
        
        StructFileMapping::init_from_handle(size, hMapFile)
    }
    
    unsafe fn init_from_handle(size: u32, hMapFile: HANDLE) -> IoResult<StructFileMapping<T>> {        
        let pBuf = MapViewOfFile(hMapFile,   // handle to map object
                       FILE_MAP_ALL_ACCESS, // read/write permission
                       0,
                       0,
                       size
                   );

        if pBuf.is_null()
        {
            let err = IoError::last_error();
            winapi::CloseHandle(hMapFile);
            return Err(err);
        }
        
        Ok(StructFileMapping::<T> { handle: hMapFile, view: pBuf })
    }
    
    pub fn get_ref<'a>(&'a self) -> &'a T {
        unsafe { mem::transmute(self.view) }
    }
    
    pub fn get_mut_ref<'a>(&'a mut self) -> &'a mut T {
        unsafe { mem::transmute(self.view) }
    }
}

#[unsafe_destructor]
impl<T> Drop for StructFileMapping<T> {
    fn drop(&mut self) {
        unsafe {
            UnmapViewOfFile(self.view);
            winapi::CloseHandle(self.handle);
        };
    }
}