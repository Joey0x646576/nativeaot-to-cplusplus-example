// LoadLibraryNativeAotPlusPlus.cpp : This file contains the 'main' function. Program execution begins and ends there.
//
#include "Windows.h"
#include <iostream>
#include "Person.h"


int main()
{
    // For simplicity, just drop the native DLL in build directory.
    const HMODULE lib = LoadLibraryA("NativeDLL.dll");

    // Creating a definition of the function object.
    typedef uintptr_t(*fnSerializedPerson)();
    typedef uintptr_t(*fnDerserializePerson)(uintptr_t, int32_t);
    typedef int32_t(*fnStringLength)(uintptr_t);

    // The function objects.
    auto serializePerson = fnSerializedPerson(GetProcAddress(lib, "GetSerializedPerson"));
    auto getStringLength = fnStringLength(GetProcAddress(lib, "GetStringLength"));
    auto deserializePerson = fnDerserializePerson(GetProcAddress(lib, "GetDeserializedPerson"));

    // Call the person functions and convert the returned pointer to a string
    uintptr_t personStringPtr = serializePerson();
    int32_t length = getStringLength(personStringPtr);
    std::string personJson(reinterpret_cast<const char*>(personStringPtr), length);

    // Call the function and convert the returned pointer to a Person object
    uintptr_t personPtr = deserializePerson(personStringPtr, length);
    Person* person = reinterpret_cast<Person*>(personPtr);

    std::cout << "Json: " << personJson << std::endl;
    std::cout << "Age: " << person->Age << std::endl;
    std::cout << "Gender: " << static_cast<int>(person->Gender) << std::endl;
}