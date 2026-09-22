#include "peltiercontroller.h"

PeltierController::PeltierController(int pPin1, int pPin2, int thPin, int KpVal) : peltierDriverPin1{pPin1}, peltierDriverPin2{pPin2}, thermistorPin{thPin}, Kp{KpVal}
{
    pinMode(thermistorPin, INPUT);
    pinMode(peltierDriverPin1, OUTPUT);
    pinMode(peltierDriverPin2, OUTPUT);

    analogReadResolution(12);
}

void PeltierController::setDesiredTemp(int temp)
{
    desiredTemp = temp;
}

void PeltierController::temperatureControl()
{
    readThermistor();
    calculateError();
    // int output = error * Kp; no pid control for now
    if(desiredTemp == 25){
            disable();
        }
    else{
        if (error > 3)
    { 
        Serial.println("Temperature too low. Heating up.");
        digitalWrite(peltierDriverPin1, HIGH);
        digitalWrite(peltierDriverPin2, LOW);
    }
    else if (error < -3)
    {
        Serial.println("Temperature too high. Cooling down.");
        digitalWrite(peltierDriverPin1, LOW);
        digitalWrite(peltierDriverPin2, HIGH);
    }
    else
    {
        Serial.println("Within desired temperature range.");
        digitalWrite(peltierDriverPin1, LOW);
        digitalWrite(peltierDriverPin2, LOW);
    }
    }
    
    Serial.println("------");
}

void PeltierController::readThermistor()
{
    float voltage = analogRead(thermistorPin) * (3.3f / 4095.0f); 
    Serial.print("Current voltage read: ");
    Serial.println(voltage);

    float thermistorValue = - (10000.0f*voltage)/(voltage - 5.0f);
    Serial.print("Current thermistor read: ");
    Serial.println(thermistorValue);
    
    currentTemp = interpolate(thermistorValue);
    Serial.print("Current temperature: ");
    Serial.println(currentTemp);
}

float PeltierController::interpolate(float resistanceValue)
{
    for (int i = 0; i < POINTS - 1; i++)
    {
        if (resistanceValue <= rt_array[i] && resistanceValue >= rt_array[i + 1])
        {
            return (temp_array[i] + (resistanceValue - rt_array[i]) *
                                        (temp_array[i + 1] - temp_array[i]) /
                                        (rt_array[i + 1] - rt_array[i]));
        }
    }
    return -999;
}

void PeltierController::calculateError()
{
    error = desiredTemp - currentTemp;
    Serial.print("Error is: ");
    Serial.println(error);
}

void PeltierController::enable()
{
    Serial.println("Peltier element enabled");
    enabled = true;
}

void PeltierController::disable()
{
    enabled = false;
    Serial.println("Peltier element disabled");
    digitalWrite(peltierDriverPin1, LOW);
    digitalWrite(peltierDriverPin2, LOW);
}

bool PeltierController::isEnabled()
{
    return enabled;
}
