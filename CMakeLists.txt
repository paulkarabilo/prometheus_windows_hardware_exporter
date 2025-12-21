cmake_minimum_required(VERSION 4.2.0 FATAL_ERROR)

project(prometheus_windows_hardware_exeporter VERSION 0.1.0 LANGUAGES CXX)

# ---- Dependencies ----

include(cmake/CPM.cmake)
CPMAddPackage(
    URI "gh:madler/zlib@1.2.11"
)
if(zlib_ADDED)
    target_include_directories(zlib 
                               PUBLIC $<BUILD_INTERFACE:${zlib_BINARY_DIR}>
                               PUBLIC $<INSTALL_INTERFACE:include>
                               PUBLIC $<BUILD_INTERFACE:${zlib_SOURCE_DIR}>)
    target_include_directories(zlibstatic 
                               PUBLIC $<BUILD_INTERFACE:${zlib_BINARY_DIR}>
                               PUBLIC $<INSTALL_INTERFACE:include>
                               PUBLIC $<BUILD_INTERFACE:${zlib_SOURCE_DIR}>)
    message(STATUS "Cmake added local zlib: ${zlib_SOURCE_DIR}")
else()
    message(FATAL_ERROR "Cmake failed to add local zlib")
endif()
add_library(ZLIB::ZLIB ALIAS zlib)
CPMAddPackage(
    URI "gh:jupp0r/prometheus-cpp@1.3.0"
    OPTIONS
        "ENABLE_TESTING OFF"
        "ENABLE_PUSH OFF"
)

add_executable(prometheus_windows_hardware_exeporter main.cpp)
target_compile_features(prometheus_windows_hardware_exeporter PRIVATE cxx_std_17)
target_link_libraries(prometheus_windows_hardware_exeporter PRIVATE
    prometheus-cpp::core
    prometheus-cpp::pull
    ZLIB::ZLIB)