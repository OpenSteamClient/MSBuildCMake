cmake_minimum_required(VERSION 3.25)
project(testproject LANGUAGES CXX) 

set(CMAKE_CXX_STANDARD 20)
include("cmake/idesupport.cmake")

add_executable(testnative "testnative/main.cpp")
add_library(testshared SHARED "testsharednative/main.cpp")
if (MINGW)
    target_link_options(testnative PRIVATE -static)
endif()

if (BUILD_ARCH STREQUAL "x86")
    set_target_properties(testnative PROPERTIES OUTPUT_NAME testnative32)
    set_target_properties(testshared PROPERTIES OUTPUT_NAME testshared32)
endif()