#include "vibration.h"

Vibration::Vibration()
{
  pinMode(PIN1, OUTPUT);
  pinMode(PIN2, OUTPUT);

  // Start I2C communication with the Multiplexer
  Wire.begin(12, 11);
  // Sets up a channel (0-15), a PWM duty cycle frequency, and a PWM resolution (1 - 16 bits)
  ledcSetup(PWM_CHANNEL_0, LEDC_BASE_FREQ_0, PWM_RESOLUTION);
  ledcSetup(PWM_CHANNEL_1, LEDC_BASE_FREQ_1, PWM_RESOLUTION);

  // ledcAttachPin(uint8_t pin, uint8_t channel);
  ledcAttachPin(PIN1, PWM_CHANNEL_0);
  ledcAttachPin(PIN2, PWM_CHANNEL_1);
}

void Vibration::SelectMPX(uint8_t bus)
{
  // Serial.println("Selecting multiplexer bus " + String(bus));
  Wire.beginTransmission(0x70); // TCA9548A address is 0x70
  Wire.write(1 << bus);         // send byte to select bus
  Wire.endTransmission();
}

void Vibration::disable(int motor)
{
  enabled[motor] = false;
  SelectMPX(mpx[motor]);
  drive(motor);
  HMD.Mode(0x43); // standby
}

void Vibration::enable(int motor)
{
  enabled[motor] = true;
  SelectMPX(mpx[motor]);
  HMD.Mode(0x03);
}

void Vibration::drive(int motor) // possibly leave out enabled
{
  if (enabled[motor])
  {
    ledcChangeFrequency(pwm[motor], frequency[motor], PWM_RESOLUTION);
    ledcWrite(pwm[motor], dutyCycle[motor]);
  }
  else
  {
    ledcWrite(pwm[motor], LOW);
  }
}

void Vibration::setFrequency(int motor, int frequency)
{
  this->frequency[motor] = frequency;

  if(frequency==0){
    disable(motor);
  }
  else{
    enable(motor);
    this->frequency[motor] = frequency;
    drive(motor);
  }
}

void Vibration::setStrength(int motor, int strength)
{
  dutyCycle[motor] = strength;

  if(strength == 0){
    disable(motor);
  }
}

bool Vibration::isEnabled()
{
  return (enabled[0] || enabled[1]);
}

void Vibration::init()
{
  SelectMPX(2);
  HMD.begin();
  HMD.Mode(0x43); // standby
  HMD.MotorSelect(0x0A);
  HMD.Library(6); // change to 6 for LRA motors

  delay(100);

  SelectMPX(3);
  HMD.begin();
  HMD.Mode(0x43); // standby
  HMD.MotorSelect(0x0A);
  HMD.Library(6); // change to 6 for LRA motors

  delay(100);
}
