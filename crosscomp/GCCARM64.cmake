# the name of the target operating system
set(CMAKE_SYSTEM_NAME Linux)
set(CMAKE_SYSTEM_PROCESSOR "aarch64")

set(TARGET "aarch64-pc-linux-gnu")
set(CMAKE_C_COMPILER_TARGET ${TARGET})
set(CMAKE_CXX_COMPILER_TARGET ${TARGET})

set(TARGET_PREFIX "aarch64-linux-gnu")
set(CMAKE_C_COMPILER ${TARGET_PREFIX}-gcc)
set(CMAKE_CXX_COMPILER ${TARGET_PREFIX}-g++)
set(CMAKE_RANLIB ${TARGET_PREFIX}-ranlib)
set(CMAKE_AR ${TARGET_PREFIX}-gcc-ar)

set(CMAKE_FIND_ROOT_PATH /usr/aarch64-linux-gnu)

set(CMAKE_SHARED_LIBRARY_LINK_CXX_FLAGS)

# adjust the default behaviour of the FIND_XXX() commands:
# search headers and libraries in the target environment, search
# programs in the host environment
set(CMAKE_FIND_ROOT_PATH_MODE_PROGRAM NEVER)
set(CMAKE_FIND_ROOT_PATH_MODE_LIBRARY ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_INCLUDE ONLY)