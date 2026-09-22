#ifndef vibration_h
#define vibration_h

#include <Arduino.h>
#include <Wire.h>
#include <Sparkfun_DRV2605L.h>
#include <algorithm>
#include <esp32-hal-ledc.h>

#define TCAADDR 0x70
#define I2C_SDA 12
#define I2C_SCL 11
#define PIN1 46 //impact
#define PIN2 3 //lf

// ------ PWM ------

// use 13 bit precision for LEDC timer
// up to 5kHz
#define LEDC_TIMER_8_BIT 13

// LEDC base frequency
#define LEDC_BASE_FREQ_0 3
#define LEDC_BASE_FREQ_1 3

// pwm channels
#define PWM_CHANNEL_0 6
#define PWM_CHANNEL_1 4

#define PWM_RESOLUTION 14

// ----------------

class Vibration
{
private:
    SFE_HMD_DRV2605L HMD;
    int fadeAmount = 5;
    void SelectMPX(uint8_t bus);
    bool enabled[2] = {false, false};
    int dutyCycle[2] = {125, 125};
    int frequency[2] = {LEDC_BASE_FREQ_0, LEDC_BASE_FREQ_1};
    int motors[2] = {PIN1, PIN2};
    int pwm[2] = {PWM_CHANNEL_0, PWM_CHANNEL_1};
    int mpx[2] = {3, 2};

public:
    Vibration();
    void setFrequency(int motor, int frequency);
    void setStrength(int motor, int strength);
    void disable(int motor);
    void enable(int motor);
    void drive(int motor);
    bool isEnabled();
    void init();
};

#endif
