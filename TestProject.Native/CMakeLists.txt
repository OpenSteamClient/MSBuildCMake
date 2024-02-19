cmake_minimum_required(VERSION 3.25)
project(testproject LANGUAGES CXX) 

set(CMAKE_CXX_STANDARD 20)
include("cmake/idesupport.cmake")

set(OSXCROSS_TARGET_DIR "" CACHE STRING "OSXCross compiler target path (set from MSBuild)")
set(OSXCROSS_TARGET "" CACHE STRING "OSXCross target (set from MSBuild)")
set(BUILD_ARCH "" CACHE STRING "Curret arch being built (set from MSBuild)")
set(CustomBuildTaskRoot "" CACHE STRING "Path to CustomBuildTask (set from MSBuild)")
set(BUILD_PLATFORM_TARGET "" CACHE STRING "Current platform being built (set from MSBuild)")
set(BUILD_RID "" CACHE STRING "Current RID being built, do not rely on this for any important logic! Does not support multi-arch builds (set from MSBuild)")
set(NATIVE_OUTPUT_FOLDER "" CACHE STRING "The place where extra build output should be placed (set from MSBuild)")

add_executable(testnative "testnative/main.cpp")
add_library(testshared SHARED "testsharednative/main.cpp")
if (MINGW)
    target_link_options(testnative PRIVATE -static)
endif()

if (BUILD_ARCH STREQUAL "x86")
    set_target_properties(testnative PROPERTIES OUTPUT_NAME testnative32)
    set_target_properties(testshared PROPERTIES OUTPUT_NAME testshared32)
endif()

if (BUILD_ARCH STREQUAL "arm64")
    set_target_properties(testnative PROPERTIES OUTPUT_NAME testnativearm)
    set_target_properties(testshared PROPERTIES OUTPUT_NAME testsharedarm)
endif()