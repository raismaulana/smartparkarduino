/**
 * --------------------------------------------------------------------------------------------------------------------
 * Example sketch/program showing how to read data from more than one PICC to serial.
 * --------------------------------------------------------------------------------------------------------------------
 * This is a MFRC522 library example; for further details and other examples see: https://github.com/miguelbalboa/rfid
 *
 * Example sketch/program showing how to read data from more than one PICC (that is: a RFID Tag or Card) using a
 * MFRC522 based RFID Reader on the Arduino SPI interface.
 *
 * Warning: This may not work! Multiple devices at one SPI are difficult and cause many trouble!! Engineering skill
 *          and knowledge are required!
 *
 * @license Released into the public domain.
 *
 * Typical pin layout used:
 * -----------------------------------------------------------------------------------------
 *             MFRC522      Arduino       Arduino   Arduino    Arduino          Arduino
 *             Reader/PCD   Uno/101       Mega      Nano v3    Leonardo/Micro   Pro Micro
 * Signal      Pin          Pin           Pin       Pin        Pin              Pin
 * -----------------------------------------------------------------------------------------
 * RST/Reset   RST          9             5         D9         RESET/ICSP-5     RST
 * SPI SS 1    SDA(SS)      ** custom, take a unused pin, only HIGH/LOW required **
 * SPI SS 2    SDA(SS)      ** custom, take a unused pin, only HIGH/LOW required **
 * SPI MOSI    MOSI         11 / ICSP-4   51        D11        ICSP-4           16
 * SPI MISO    MISO         12 / ICSP-1   50        D12        ICSP-1           14
 * SPI SCK     SCK          13 / ICSP-3   52        D13        ICSP-3           15
 *
 */

#include <MFRC522.h>
#include <Servo.h>
#include <SPI.h>

#define RST_PIN         5          // Configurable, see typical pin layout above
#define SS_1_PIN        11         // Configurable, take a unused pin, only HIGH/LOW required, must be diffrent to SS 2
#define SS_2_PIN        12          // Configurable, take a unused pin, only HIGH/LOW required, must be diffrent to SS 1

#define NR_OF_READERS   2

byte ssPins[] = {SS_1_PIN, SS_2_PIN};

MFRC522 mfrc522[NR_OF_READERS];   // Create MFRC522 instance.
Servo servoMasuk, servoKeluar;

int posServo = 90, isObstacle = HIGH;
int pinIRa = 6, pinIRb = 7, pinIRc = 8, pinIRd = 9, pinIRin = 4, pinIRout = 10, pinBuzzerin = 31, pinBuzzerout = 30;
String kartuMasuk, dataTerima;
//String kartuTerdaftar[][2] = {
//  {"48411419422792128", "rais"},
//  {"134225222249", "raroh"}
//};

/**
 * Initialize.
 */
void setup() {

  Serial.begin(9600); // Initialize serial communications with the PC
  while (!Serial);    // Do nothing if no serial port is opened (added for Arduinos based on ATMEGA32U4)

  SPI.begin();        // Init SPI bus

  for (uint8_t reader = 0; reader < NR_OF_READERS; reader++) {
    mfrc522[reader].PCD_Init(ssPins[reader], RST_PIN); 
  }
//servo
  servoMasuk.attach(2);
  servoKeluar.attach(3);
//end servo

//IR
  
  pinMode(pinIRa, INPUT);
  pinMode(pinIRb, INPUT);
  pinMode(pinIRc, INPUT);
  pinMode(pinIRd, INPUT);
  pinMode(pinIRin, INPUT);
  pinMode(pinIRout, INPUT);

//IR end

//buzzer
  pinMode(pinBuzzerin, OUTPUT);
  pinMode(pinBuzzerout, OUTPUT);
  digitalWrite(pinBuzzerin, HIGH);
  digitalWrite(pinBuzzerout, HIGH);
//buzzer end
}

/**
 * Main loop.
 */
void loop() {
  if (Serial.available() > 0){
    dataTerima = Serial.readStringUntil('\n');
    //dataTerima = "1";
    if (dataTerima == "daftar"){
      daftar();
    }
    else if (dataTerima == "1"){
      monitoring();
    }
  }
}

void daftar(){
    if (mfrc522[0].PICC_IsNewCardPresent() && mfrc522[0].PICC_ReadCardSerial()) {
        kartuMasuk = ambil_id(mfrc522[0].uid.uidByte, mfrc522[0].uid.size);
        Serial.print("kartu~");
        Serial.print(kartuMasuk);
        Serial.println("~i");
        
        // Halt PICC
        mfrc522[0].PICC_HaltA();
        // Stop encryption on PCD
        mfrc522[0].PCD_StopCrypto1();
    }
}
/**
 * Monitoring
 */
void monitoring(){
  int a = digitalRead(pinIRa), b = digitalRead(pinIRb), c = digitalRead(pinIRc), d = digitalRead(pinIRd), in, out = digitalRead(pinIRout);
  for (uint8_t reader = 0; reader < NR_OF_READERS; reader++) {
    // Look for new cards
  
    if (mfrc522[reader].PICC_IsNewCardPresent() && mfrc522[reader].PICC_ReadCardSerial()) {
      if(reader == 0){
        kartuMasuk = ambil_id(mfrc522[reader].uid.uidByte, mfrc522[reader].uid.size);
        //Serial.println(kartuMasuk);  
        
        if (cek_kartu(kartuMasuk, "i") == true) {         
          Serial.println("gerbang~"+kartuMasuk+"~i");
          aksi_gerbang("buka", servoMasuk, "masuk");
        } else {
          Serial.println("gerbang~"+kartuMasuk+"~x");
          aksi_gerbang("tutup", servoMasuk, "masuk");
        }//if (kartu_masuk ==
        

        // Halt PICC
        mfrc522[reader].PICC_HaltA();
        // Stop encryption on PCD
        mfrc522[reader].PCD_StopCrypto1();
      } 
      else {
       
        kartuMasuk = ambil_id(mfrc522[reader].uid.uidByte, mfrc522[reader].uid.size);
        //Serial.println(kartuMasuk);
        if (cek_kartu(kartuMasuk, "o") == true) {
          Serial.println("gerbang~"+kartuMasuk+"~o");
          aksi_gerbang("buka", servoKeluar, "keluar");
        } else {
          Serial.println("gerbang~"+kartuMasuk+"~x");
          aksi_gerbang("tutup", servoKeluar, "keluar");
        }//if (kartu_masuk ==
         
        // Halt PICC
        mfrc522[reader].PICC_HaltA();
        // Stop encryption on PCD
        mfrc522[reader].PCD_StopCrypto1();
      }
    } //if (mfrc522[reader].PICC_IsNewC
  } //for(uint8_t reader

  Serial.print("ir~");
  baca_IR(pinIRa);
  Serial.print(":");
  baca_IR(pinIRb);
  Serial.print(":");
  baca_IR(pinIRc);
  Serial.print(":");
  baca_IR(pinIRd);
  Serial.println();
}

/**
 * Mengambil id kartu.
 */
String ambil_id(byte *buffer, byte bufferSize) {
  String idKartu = "";
  for (byte i = 0; i < bufferSize; i++) {
    idKartu += buffer[i];
  }
  return idKartu;
}

//bool cek_kartu(String kartuMasuk)  {
//  for(int i = 0; i < sizeof(kartuTerdaftar)/12; i++){
//    if(kartuMasuk == kartuTerdaftar[i][0]){
//      return true;
//    }
//  }
//  return false;
//}

bool cek_kartu(String kartuMasuk, String ket){
  Serial.print("kartu~");
  Serial.println(kartuMasuk);
  int i = 0;
  while(i == 0){
    i=0;
    if(Serial.available()>0){
      dataTerima = Serial.readStringUntil('\n');
      if(dataTerima == "true"){
        i=1;
        return true;
      }
      else if (dataTerima == "false"){
        i=1;
        return false;
      }
      else{
        i=0;
      }
    }
  }
}


void aksi_gerbang(String aksi, Servo gerbang, String jalur) {
  int a = digitalRead(pinIRa), b = digitalRead(pinIRb), c = digitalRead(pinIRc), d = digitalRead(pinIRd), in, out = digitalRead(pinIRout);
  if (jalur == "masuk") {
    if (aksi == "buka") {
      if(a == LOW and b == LOW and c == LOW and d == LOW)  {
        for(int i = 0; i < 2; i++) {
          digitalWrite(pinBuzzerin, LOW);
          delay(200);
          digitalWrite(pinBuzzerin, HIGH);
          delay(200);
        }
      } else {
        digitalWrite(pinBuzzerin, HIGH);
//      Serial.print("BUKA PINTU!");
        gerbang.write(180);              // tell servo to go to position in variable 'pos'
        delay(2750);
        in = digitalRead(pinIRin);
        if(in == LOW)  {
          while(in == LOW){
          in = digitalRead(pinIRin);
          digitalWrite(pinBuzzerin, LOW);
          delay(300);
          digitalWrite(pinBuzzerin, HIGH);
          delay(100);
          }    
          digitalWrite(pinBuzzerin, HIGH);
          delay(1500);
          gerbang.write(90);
        } else {
          gerbang.write(90);
        }
      }
    } else {
//      Serial.print("KARTU TIDAK TERDAFTAR");
      digitalWrite(pinBuzzerin, LOW);
      delay(5000);
      digitalWrite(pinBuzzerin, HIGH); 
    }
  } else {
    if (aksi == "buka") {
      digitalWrite(pinBuzzerout, HIGH);
      gerbang.write(180);              // tell servo to go to position in variable 'pos'
      delay(2750);
      out = digitalRead(pinIRout);
      if(out == LOW)  {
        while(out == LOW){
        out = digitalRead(pinIRout);
        digitalWrite(pinBuzzerout, LOW);
        delay(300);
        digitalWrite(pinBuzzerout, HIGH);
        delay(100);
        }    
        digitalWrite(pinBuzzerout, HIGH);
        delay(1500);
        gerbang.write(90);
      } else {
        gerbang.write(90);
      }
    } else {
      digitalWrite(pinBuzzerout, LOW);
      delay(5000);
      digitalWrite(pinBuzzerout, HIGH); 
    }
  }

}//void aksi_gerbanng

void baca_IR(int pinIR)  {
  isObstacle = digitalRead(pinIR);
  String pin = String(pinIR);
  if (isObstacle == LOW)
  {
    Serial.print("isi");
  }
  else
  {
    Serial.print("kosong");
  }
}
