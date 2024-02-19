set(CMAKE_SYSTEM_NAME Windows)
set(CMAKE_SYSTEM_PROCESSOR "x86_64")
set(CMAKE_SYSTEM_VERSION "10")

set(CMAKE_CROSSCOMPILING true)

set(TARGET "x86_64-w64-mingw32") 
set(CMAKE_C_COMPILER_TARGET ${TARGET})
set(CMAKE_CXX_COMPILER_TARGET ${TARGET})

set(TARGET_PREFIX "x86_64-w64-mingw32")
set(CMAKE_C_COMPILER ${TARGET_PREFIX}-gcc)
set(CMAKE_CXX_COMPILER ${TARGET_PREFIX}-g++)
set(CMAKE_RANLIB ${TARGET_PREFIX}-ranlib)
set(CMAKE_RC_COMPILER ${TARGET_PREFIX}-windres)
set(CMAKE_AR ${TARGET_PREFIX}-gcc-ar)

set(CMAKE_FIND_ROOT_PATH /usr/x86_64-w64-mingw32)

set(CMAKE_SHARED_LIBRARY_LINK_CXX_FLAGS)