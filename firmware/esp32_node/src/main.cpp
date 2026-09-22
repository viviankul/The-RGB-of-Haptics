#include <Wire.h>
#include "vibration.h"
#include <Arduino.h>
#include <ESP32Servo.h>
#include "peltiercontroller.h"
#include <WiFi.h>
#include <WiFiUdp.h>

// ------------------------- WIFI -----------------------------


#define ssid "your-ssid"         //WiFi SSID
#define password "your-password"           //WiFi password

// Server settings
#define serverIP "192.168.245.91"      // Unity server's IP address  
#define serverPort 11000             // Unity server's port


WiFiUDP udp;
char incomingPacket[64];  // Buffer to hold incoming message

int command;
int parameter_0;
int parameter_1;
int parameter_2;


// ------------------------- HARDWARE -----------------------------
Vibration vibration;

Servo servoMotor;
// int pos = 0;

PeltierController peltierController(4,6,1,1);

// Flex sensor
int flexPin = 2;
bool flexEnabled = false;

// -----------------------------------------------------------------
TaskHandle_t TemperatureAndFlexControlTask;


void connectWifi(){
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("");
  Serial.println("WiFi connected.");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());
  udp.begin(serverPort);
}

void sendMessage(float message){
  // char buffer[10];
  // sprintf (buffer, "%d", message);
  String buffer = String(message);
  udp.beginPacket(serverIP, serverPort);
  udp.print(buffer);
  udp.endPacket();
}


void sendFlexValue(){
  float flexValue= analogRead(flexPin) * (3.3f / 4095.0f);;
  sendMessage(flexValue);
  Serial.print("Flex Value is: ");
  Serial.println(flexValue);
}

void temperatureAndFlexControl(void * pvParameters){
  while(1){
      if(peltierController.isEnabled()){
        peltierController.temperatureControl();
      }
  // if(flexEnabled){
  //     sendFlexValue();
  // }
  vTaskDelay(1000);
  }
}

const char * receiveMessage() {
  int packetSize = udp.parsePacket();  // Check for an incoming UDP packet
  if (packetSize) {
    
    int len = udp.read(incomingPacket, 255);
    if (len > 0) {
      incomingPacket[len] = '\0';  // Null-terminate the string
    }
    Serial.print("Received: ");
    Serial.println(String(incomingPacket));
    return incomingPacket;  // Return as a String
  }
  return "";  // Return an empty String if no message was received
}

void setup() {

  Serial.begin(9600);
  connectWifi();

  xTaskCreatePinnedToCore(
                temperatureAndFlexControl,   /* Task function. */
                "TemperatureAndFlexControl",     /* name of task. */
                3000,       /* Stack size of task */
                NULL,        /* parameter of the task */
                0,           /* priority of the task */
                &TemperatureAndFlexControlTask,      /* Task handle to keep track of created task */
                0);          /* pin task to core 1 */
  delay(500);

  servoMotor.attach(19); 



  vibration.init();

  peltierController.setDesiredTemp(30);

  Serial.print("Free heap: ");
Serial.println(ESP.getFreeHeap());


}
  
void loop() {
  // ------------------------- VIBRATION -----------------------------

  // vibration.setFrequency(0, 10);
  // vibration.setStrength(0, 255);
  // vibration.enable(0);
  // vibration.disable(1);
  // delay(3000);
  // vibration.setFrequency(0, 3);
  // vibration.setStrength(0, 125);
  // // vibration.disable(0);
  //   vibration.enable(0);

  // vibration.enable(1);
  // delay(3000);

  // ------------------------- SERVO -----------------------------

  // for (pos = 0; pos <= 180; pos += 1) { // goes from 0 degrees to 180 degrees
  //   // in steps of 1 degree
  //   servoMotor.write(pos);              // tell servo to go to position in variable 'pos'
  //   delay(15);                       // waits 15ms for the servo to reach the position
  // }
  // for (pos = 180; pos >= 0; pos -= 1) { // goes from 180 degrees to 0 degrees
  //   servoMotor.write(pos);              // tell servo to go to position in variable 'pos'
  //   delay(15);                       // waits 15ms for the servo to reach the position
  // }

  // ------------------------- PELTIER -----------------------------
  // peltierController.enable();


  // if (peltierController.isEnabled()){
  //     peltierController.temperatureControl();
  // }

// ------------------------- UDP COMM -----------------------------

/* 
  * Stop: 0,0,0,0

  * Set: 1,%d,%d,%d
    - desired temperature [20-40]
    - Titan haptic motor 1: set strength [0-255] (impact)
    - Titan haptic motor 2: set strength [0-255]

  * Continuous: 2,%d,%d,$d
    - Titan haptic motor 1: set frequency [0-100]
    - Titan haptic motor 2: set frequency [0-100]
    - Servo angle [0-180] 
*/

  if(sscanf(receiveMessage(), "%d,%d,%d,%d",&command, &parameter_0, &parameter_1, &parameter_2) ==4){
    switch(command){
      case 0:
        peltierController.setDesiredTemp(25);
        vibration.disable(0);
        vibration.disable(1);
        servoMotor.write(90);

        break;

      case 1: // Initial haptic activation before any user movement. Only sent once.
        
        peltierController.enable();
        peltierController.setDesiredTemp(parameter_0);

        vibration.setStrength(0, parameter_1);
        vibration.setStrength(1, parameter_2);
        
        break;
      
      case 2: // Adapt haptic parameters to user's movement. Sent continuously.

        // Set frequency to 0 if within 1-2 Hz range (3 Hz is minimum frequency)
        if(parameter_0 > 0 && parameter_0 < 3){parameter_0 = 0;}
        if(parameter_1 > 0 && parameter_1 < 3){parameter_1 = 0;}

        vibration.setFrequency(0, parameter_0);
        vibration.setFrequency(1, parameter_1);
        servoMotor.write(parameter_2);

        break;

      default:
        Serial.println("Incorrect input");
        break;
    }
  }

}
