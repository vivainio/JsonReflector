*** Settings ***
Documentation     Example for JsonReflector
Library    JsonReflectorLib
*** Test Cases ***    
Ping the remote server
    Log    Hello world
    ${pong}=    App.DemoDispatchClass Ping
    Log    ${pong}
