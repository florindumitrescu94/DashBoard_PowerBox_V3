/* Written by: Florin Dumitrescu
 *  for ascom_powerbox_and_ambientconditions built with
 *  Arduino NANO V3
 *  January 2024
 *  DashBoard Arduino code
 */
//INCLUDE LIBRARIES

#include <DHTStable.h>;
#include <EEPROM.h>;
DHTStable DHT;





//CODE VARIABLES
int DC1=0;
int DC2=0;
int DC3=0;
int DC45=0;
int EXT1=0;
int EXT2=0;
int PWM1=0;
int PWM2=0;
int level_a = 15; // MID by default
int level_b = 15; // MID by default
int levela_show = 1;
int levelb_show = 1;
int PWM_AUTO_A = 0;
int PWM_AUTO_B = 0;
float TEMP = 0.0;
int HUM_REL = 0;
float DEWPOINT = 0.0;
float DP_OFFSET = 2;
float VOLT = 0.00;
float VOLT_TEMP = 0.00;
int PIN_VALUE_A = 0;
int PIN_VALUE_V = 0;
float CURRENT_SAMPLE_SUM = 0.0;
int AVERAGE_COUNT = 0;
float AMP_AVERAGE[150];
float AVGAMP;
float AMP = 0.00;
float PWR = 0.00;
long int time1=0;
long int time2=0;
double PWR_TOTAL_S=0.00;
float PWR_TOTAL=0.00;
double ACS_resolution;


//VARIABLES FOR SERIALQUEUE
#define QUEUELENGTH         10             // number of commands that can be saved in the serial queue
#define MAXCOMMAND          42            // max length of a command
#define EOFSTR              '\n'
#define EOCOMMAND           '#'           // defines the end character of a command
#define SOCOMMAND           '>'           // defines the start character of a command
#define REFRESH             200           // read values every REFRESH milliseconds
#define PWMREFRESH          1000        // adjust PWM every 60 seconds
char* queue[QUEUELENGTH];
int queueHead = -1;
int queueCount = 0;
enum FSMStates { stateIdle, stateNtc, statePower, stateAutoPWM };
int idx = 0;                              // index into the command string
long int now;                             // now time in millis
long int last;                            // last time in millis
long int lastm;                           // last time we updated the dewheaters in millis
String line;                              // command buffer


//CHANGE ACS_VARIANT WITH YOUR ACS CHIP VARIANT (5A, 20A, 30A)
const int ACS_Variant = 20; //A
//PINS
const int DC1_PIN = 4 ;
const int DC2_PIN = 5;
const int DC3_PIN = 6;
const int DC45_PIN = 3;
const int EXT1_PIN = 7;
const int EXT2_PIN = 8;
const int PWM1_PIN = 10;
const int PWM2_PIN = 9;
const int DHT22_DATA = 2;
const int VM = A0;
const int AM = A1;


//-----------------------------------------------------------------------
// Utility functions
//-----------------------------------------------------------------------

char* pop() {
  --queueCount;
  return queue[queueHead--];
}


void push(char command[MAXCOMMAND]) {
  queueCount++;
  queueHead++;
  strncpy(queue[queueHead], command, MAXCOMMAND);
}


//ARDUINO INITIALIZATION
void setup()
{
  //SET PIN MODES
    pinMode(DC1_PIN, OUTPUT);
    pinMode(DC2_PIN, OUTPUT);
    pinMode(DC3_PIN, OUTPUT);
    pinMode(DC45_PIN, OUTPUT);
    pinMode(PWM1_PIN, OUTPUT);
    pinMode(PWM2_PIN, OUTPUT);
    pinMode(EXT1_PIN, OUTPUT);
    pinMode(EXT2_PIN, OUTPUT);
    pinMode(DHT22_DATA, INPUT);
    pinMode(VM, INPUT);
    pinMode(AM, INPUT);


  //INITIALIZE PIN STATE FROM EEPROM
   //
   if (EEPROM.read(40) != 255)
   // eerpom initializes with 255 on all values on a fresh arduino. That throws off ASCOM driver. 
   //To fix this, we check address 40 to see if it is 255. That means this is a first boot for the arduino. 
   //It skips the values, setting them to 0 as in the variable declaration step.
   //We then set the address 40 to 1 so that next time, we know it is not a first run and the EEPROM can be read
   {
    DC1=EEPROM.read(3);
    DC2=EEPROM.read(4);
    DC3=EEPROM.read(5);
    DC45=EEPROM.read(6);
    EXT1=EEPROM.read(7);
    EXT2=EEPROM.read(8);
    level_a=EEPROM.read(9);
    level_b=EEPROM.read(10);
    PWM1=EEPROM.read(1);
    PWM2=EEPROM.read(2);
   }
   EEPROM.update(40,1);//on first run on a fresh arduino, update address 40 to 1, so it will run the eeprom reads
    digitalWrite(DC1_PIN,DC1);
    digitalWrite(DC2_PIN,DC2);
    digitalWrite(DC3_PIN,DC3);
    digitalWrite(DC45_PIN,DC45);
    digitalWrite(EXT1_PIN,EXT1);
    digitalWrite(EXT2_PIN,EXT2);
    analogWrite(PWM1_PIN,PWM1);
    analogWrite(PWM2_PIN,PWM2);

    for ( int i=0; i < QUEUELENGTH; i++)
      queue[i] = (char*)malloc(MAXCOMMAND);
    line.reserve(MAXCOMMAND);
    
    Serial.begin(115200);
    Serial.flush();

    switch (ACS_Variant){
      case 5: 
        ACS_resolution = 0.185;
        break;
      case 20: 
        ACS_resolution = 0.100;
        break;
      case 30:
        ACS_resolution = 0.066;
        break;
    }
    // initialize our delays
    now = millis();
    last = now;
    lastm = now;
}

void clearSerialPort() {
  while ( Serial.available() )
    Serial.read();
}

// SerialEvent occurs whenever new data comes in the serial RX.
// you should really consider a start of command character.
void serialEvent() {

  // '#' ends the command, do not store these in the command buffer
  // read the command until the terminating # character
  char buf[MAXCOMMAND];
  while ( Serial.available() )
  {
    char inChar = Serial.read();
    switch ( inChar )
    {
      case '>':     // soc, reinit line
        // memset(line, 0, MAXCOMMAND);
        line = "";
        idx = 0;
        break;
      case '#':     // eoc
        line.toCharArray(buf, MAXCOMMAND);
        idx = 0;
        push(buf);
        break;
      default:      // anything else
        if ( idx < MAXCOMMAND - 1) {
          line += inChar;
        }
        break;
    }
  }
}


void processSerialCommand() {
  // this should never happen
  if ( queueCount == 0 )
    return;

  String cmd = String(pop());


//READS AND EXECUTES SERIAL COMMANDS FROM ASCOM DRIVER
  if (cmd.substring(0,6) == "SETDC1") SET_DC_PIN(1,(cmd.substring((cmd.indexOf('_')+1),cmd.indexOf('_')+2)).toInt()); 
  else if (cmd.substring(0,6) == "SETDC2") SET_DC_PIN(2,(cmd.substring((cmd.indexOf('_')+1),cmd.indexOf('_')+2)).toInt());
  else if (cmd.substring(0,6) == "SETDC3") SET_DC_PIN(3,(cmd.substring((cmd.indexOf('_')+1),cmd.indexOf('_')+2)).toInt()); 
  else if (cmd.substring(0,6) == "SETDC4") SET_DC_PIN(4,(cmd.substring((cmd.indexOf('_')+1),cmd.indexOf('_')+2)).toInt());  
  else if (cmd.substring(0,7) == "SETEXT1") SET_EXT_PIN(1,(cmd.substring((cmd.indexOf('_')+1),cmd.indexOf('_')+2)).toInt()); 
  else if (cmd.substring(0,7) == "SETEXT2") SET_EXT_PIN(2,(cmd.substring((cmd.indexOf('_')+1),cmd.indexOf('_')+2)).toInt()); 
  else if (cmd == "SETAUTOPWMA_1") {
      PWM_AUTO_A = 1;
  }
  else if (cmd == "SETAUTOPWMA_0") {
    PWM_AUTO_A = 0;
  }
  else if (cmd == "SETAUTOPWMB_1") {
      PWM_AUTO_B = 1;
  }
  else if (cmd == "SETAUTOPWMB_0") {
    PWM_AUTO_B = 0;
  }
  else if (cmd.substring(0,9) == "SETLEVELA") SET_LEVEL(1,(cmd.substring((cmd.indexOf('_')+1),cmd.indexOf('_')+2)).toInt());
  else if (cmd.substring(0,9) == "SETLEVELB") SET_LEVEL(2,(cmd.substring((cmd.indexOf('_')+1),cmd.indexOf('_')+2)).toInt());
else if (cmd == "WRITEEEPROM") //writes current values to EEPROM before disconnecting
  {
    EEPROM.update(1,PWM1);
    EEPROM.update(2,PWM2);
    EEPROM.update(3,DC1);
    EEPROM.update(4,DC2);
    EEPROM.update(5,DC3);
    EEPROM.update(6,DC45);
    EEPROM.update(7,EXT1);
    EEPROM.update(8,EXT2);
    EEPROM.update(9,level_a);
    EEPROM.update(10,level_b);
  } 
  else if (cmd == "READEEPROM") //reads existing PWM1 and 2 values from EEPROM and sets them on PWM1 and 2 on connection
  {
    PWM1=EEPROM.read(1);
    PWM2=EEPROM.read(2);
    SET_PWM_POWER(1,PWM1);
    SET_PWM_POWER(2,PWM2);
  }
  
  else if (cmd.substring(0,7) == "SETPWM1") SET_PWM_POWER(1,cmd.substring((cmd.indexOf('_')+1),(cmd.indexOf('_')+4)).toInt());
  else if (cmd.substring(0,7) == "SETPWM2") SET_PWM_POWER(2,cmd.substring((cmd.indexOf('_')+1),(cmd.indexOf('_')+4)).toInt());
  else if (cmd == "REFRESH")
  {
    Serial.print(DC1); //0
    Serial.print(":");
    Serial.print(DC2); //1
    Serial.print(":");
    Serial.print(DC3); //2
    Serial.print(":");
    Serial.print(DC45); //3
    Serial.print(":");
    Serial.print(PWM1); //4
    Serial.print(":");
    Serial.print(PWM2); //5
    Serial.print(":");
    Serial.print(TEMP); //6
    Serial.print(":");
    Serial.print(HUM_REL); //7
    Serial.print(":");
    Serial.print(DEWPOINT); //8
    Serial.print(":");
    Serial.print(VOLT); //9
    Serial.print(":");
    Serial.print(AMP); //10
    Serial.print(":");
    Serial.print(PWR); //11
    Serial.print(":");
    Serial.print(PWR_TOTAL); //12
    Serial.print(":");
    Serial.print(PWM_AUTO_A); //13
    Serial.print(":");
    Serial.print(PWM_AUTO_B); //14
    Serial.print(":");
    Serial.print(levela_show); //15
    Serial.print(":");
    Serial.print(levelb_show); //16
    Serial.print(":");
    Serial.print(EXT1); //17
    Serial.print(":");
    Serial.print(EXT2); //18
    Serial.print("#");
  }
}


//LOOP TO READ SERIAL COMMANDS
void loop()
{
  GET_POWER(); //needs to run constantly for correct calculations
  static byte FSMState = stateIdle;

  if ( queueCount >= 1 ) {               // check for serial command
    processSerialCommand();
  }

  switch (FSMState) {
    case stateIdle:
      // wait REFRESH milliseconds between Read cycles
      now = millis();
      if ( now > last + REFRESH )
        FSMState = statePower;
      else
        FSMState = stateIdle;
      break;
    case statePower:
      GET_AMBIENT();
      now = millis();
      FSMState = stateAutoPWM;
      last = now;
      break;
    case stateAutoPWM:
      now = millis();
      if ( now > lastm + PWMREFRESH ) {
        RUN_AUTO_PWM_A();
        RUN_AUTO_PWM_B();
        lastm = now;
      }
      last = now;
      FSMState = stateIdle;
      break;
  }
} 
// END loop


//SET/GET FUNCTIONS


//SET DC PIN

void SET_DC_PIN(int pin, int state){
     if (state == 0)
     { 
       if (pin == 1) 
       {
        digitalWrite(DC1_PIN, LOW);
        DC1 = 0;
        
       }
       else if (pin == 2) 
       {
        digitalWrite(DC2_PIN, LOW);
        DC2 = 0;
        
       }
       else if (pin == 3) 
       {
        digitalWrite(DC3_PIN, LOW);
        DC3 = 0;
        
       }
       else if (pin == 4) 
       {
        digitalWrite(DC45_PIN, LOW);
        DC45 = 0;
        
       }
       }
      else if (state == 1) 
      {
       if (pin==1) 
       {
        digitalWrite(DC1_PIN, HIGH);
        DC1 = 1;
        
       }
       else if (pin == 2) 
       {
        digitalWrite(DC2_PIN, HIGH);
        DC2 = 1;
        
       }
       else if (pin == 3) 
       {
        digitalWrite(DC3_PIN, HIGH);
        DC3 = 1;
         
       }
       else if (pin == 4) 
       {
      digitalWrite(DC45_PIN, HIGH);
      DC45 = 1;
       
       }
      }
}
//END SET DC PIN



// SET EXT PIN
void SET_EXT_PIN(int pin, int state){
     if (state == 0)
     { 
       if (pin == 1) 
       {
        digitalWrite(EXT1_PIN, LOW);
        EXT1 = 0;
         
       }
       else if (pin == 2) 
       {
        digitalWrite(EXT2_PIN, LOW);
        EXT2 = 0;
        
       }
       }
      else if (state == 1) 
      {
       if (pin==1) 
       {
        digitalWrite(EXT1_PIN, HIGH);
        EXT1 = 1;
        
       }
       else if (pin == 2) 
       {
        digitalWrite(EXT2_PIN, HIGH);
        EXT2 = 1;
        
       }
      }
}
//END SET EXT PIN



//SET PWM POWER
void SET_PWM_POWER(int pwmno,int state) {
    int pwm_value = (state*255)/100;
    if (pwmno == 1)
    {
    analogWrite(PWM1_PIN, pwm_value);
    PWM1 = state;
    }
    else if (pwmno == 2)
    {
    analogWrite(PWM2_PIN, pwm_value);
    PWM2 = state;
    }
}
//END SET PWM POWER





// RUN PWM AUTO 
void RUN_AUTO_PWM_A() {
  if (PWM_AUTO_A == 1)
  {
    double delta_td = TEMP-DEWPOINT;
    int delta_t = round(delta_td);
    if (delta_t <= 10)
    {
      int pwm_val = (10-delta_t) * level_a;
      if (pwm_val >=100)
      {
        pwm_val = 100;
      }
     SET_PWM_POWER(1,pwm_val);
     //SET_PWM_POWER(2,pwm_val);
    }
    else 
    {
      SET_PWM_POWER(1,0);
      //SET_PWM_POWER(2,0);
    }
  }
}
void RUN_AUTO_PWM_B()
{
  if (PWM_AUTO_B == 1)
  {
    double delta_td = TEMP-DEWPOINT;
    int delta_t = round(delta_td);
    if (delta_t <= 10)
    {
      int pwm_val = (10-delta_t) * level_b;
      if (pwm_val >=100)
      {
        pwm_val = 100;
      }
     SET_PWM_POWER(2,pwm_val);

    }
    else 
    {
      SET_PWM_POWER(2,0);

    }
  }
}
//END SET AUTO PWM POWER





//MEASURE AND CALCULATE POWER USAGE
  void GET_POWER() {
    time1=millis();
    if (AVERAGE_COUNT == 150) AVERAGE_COUNT = 0;
    float VOLTAGE_SAMPLE_SUM=0;
    for (int v=0;v<150;v++) 
    {
      PIN_VALUE_V =  analogRead(VM);
      VOLTAGE_SAMPLE_SUM +=PIN_VALUE_V;     
    }
    VOLTAGE_SAMPLE_SUM /=150;
    VOLT_TEMP = (VOLTAGE_SAMPLE_SUM * 5.0) / 1024.0;   
    VOLT = VOLT_TEMP / 0.0894;        
    if (VOLT < 0.1) VOLT=0.0;    
    CURRENT_SAMPLE_SUM=0;
    for (int i=0;i<150;i++)
    {
      PIN_VALUE_A= analogRead(AM);
      CURRENT_SAMPLE_SUM = CURRENT_SAMPLE_SUM + PIN_VALUE_A;
    }
    AMP_AVERAGE[AVERAGE_COUNT] = (2.494 - ((CURRENT_SAMPLE_SUM/150)*(5.0/1024.0))) / ACS_resolution;
    AVGAMP=0;
    for (int c=0;c<150;c++) AVGAMP += AMP_AVERAGE[c];
    AMP = (AVGAMP*-1)/150;
    if (AMP< 0.01) AMP=0.0;
    PWR_TOTAL_S = PWR_TOTAL_S + ((((PWR+(VOLT*AMP))/2)*(time1-time2))/1000); // total power in W*s between cycles
    PWR = VOLT * AMP;
    AVERAGE_COUNT = AVERAGE_COUNT + 1;
    time2=millis();
    PWR_TOTAL_S = PWR_TOTAL_S + ((PWR*(time2-time1))/1000); // total power in W*s
    PWR_TOTAL = PWR_TOTAL_S/3600;  // Total power used in W*h                      
    //Serial.println(AVGAMP);             
}
//END GET POWER USAGE



//GET AMBIENT CONDITIONS
  void GET_AMBIENT() {
  int PIN_VALUE_T = DHT.read22(DHT22_DATA);
   HUM_REL = DHT.getHumidity();
   TEMP = DHT.getTemperature();
   DEWPOINT  = (TEMP - (100 - HUM_REL) / 5);
 }
 
 
 
 
 //CORRECT PWM VALUE
 void SET_PWM_VALUE(int pwmno, int value){
  value = CORRECT_PWM(value);
  int pwm_pin = (value*255)/100;
  if (pwmno == 1) analogWrite (PWM1, pwm_pin);
  if (pwmno == 2) analogWrite (PWM2, pwm_pin);
 }
 int CORRECT_PWM(int state){
 if (state > 100) return 100;
 if (state < 0) return 0;
 else return state;
}

void SET_LEVEL(int port, int level)
{
  int convert;
  if (level == 1) convert = 10;
  else if (level == 2) convert = 15;
  else if (level == 3) convert = 20; 
  if (port == 1) {level_a = convert; levela_show = level;}
  if (port == 2) {level_b = convert; levelb_show = level;}
}
 
