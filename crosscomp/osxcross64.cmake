# Problem: CMake runs toolchain files multiple times, but can't read cache variables on some runs.
# Workaround: On first run (in which cache variables are always accessible), set an intermediary environment variable.

if (OSXCROSS_TARGET)
    # Environment variables are always preserved.
    set(ENV{_OSXCROSS_TARGET} "${OSXCROSS_TARGET}")
else ()
    set(OSXCROSS_TARGET "$ENV{_OSXCROSS_TARGET}")
endif ()

if (OSXCROSS_TARGET_DIR)
    # Environment variables are always preserved.
    set(ENV{_OSXCROSS_TARGET_DIR} "${OSXCROSS_TARGET_DIR}")
else ()
    set(OSXCROSS_TARGET_DIR "$ENV{_OSXCROSS_TARGET_DIR}")
endif ()


# the name of the target operating system
set(CMAKE_SYSTEM_NAME Darwin)
set(APPLE true)

set(CMAKE_CROSSCOMPILING true)

# which compilers to use for C and C++
set(OSXCROSS_TARGET_ARCH x86_64)
set(CMAKE_C_COMPILER ${OSXCROSS_TARGET_ARCH}-apple-${OSXCROSS_TARGET}-clang)
set(CMAKE_CXX_COMPILER ${OSXCROSS_TARGET_ARCH}-apple-${OSXCROSS_TARGET}-clang++)
set(CMAKE_RANLIB ${OSXCROSS_TARGET_ARCH}-apple-${OSXCROSS_TARGET}-ranlib)
set(CMAKE_AR ${OSXCROSS_TARGET_ARCH}-apple-${OSXCROSS_TARGET}-ar)

set(TARGET ${OSXCROSS_TARGET_ARCH}-apple-${OSXCROSS_TARGET}) 
set(CMAKE_C_COMPILER_TARGET ${TARGET})
set(CMAKE_CXX_COMPILER_TARGET ${TARGET})

# here is the target environment located
set(CMAKE_FIND_ROOT_PATH ${OSXCROSS_TARGET_DIR})

# adjust the default behaviour of the FIND_XXX() commands:
# search headers and libraries in the target environment, search
# programs in the host environment
set(CMAKE_FIND_ROOT_PATH_MODE_PROGRAM NEVER)
set(CMAKE_FIND_ROOT_PATH_MODE_LIBRARY ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_INCLUDE ONLY)