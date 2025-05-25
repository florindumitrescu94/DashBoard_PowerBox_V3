# DashBoard PowerBox V3 

The third evolution of the DashBoard PowerBox project, upgraded based on personal experiences and others' suggestion.
It incorporates the same basic construction as the V2 box, with easily sourceable components, but adds a few new elements. 

## Separate DC Port control
This box is now capable of controling separate DC ports, with 3 of them having their own separate switches and another 2 linked to the same switch. 

## Always On DC Port
Also, I've included an "Always On" port, which passes through the current and voltage meters, so its' power consumption will be taken into account. 

## Isolated external control
I've added two optocouplers which can be used to control external equipment (see explanation below). Using optocouplers ensures good insulation between the Arduino and the external equipment, so that a malfunction of one does not affect the other. This feature will allow the control of external non-ASCOM connected equipment, like relays or other types of switches to control devices around the observatory. The case was not designed to incorporate any connectors for the External switches, but a 3.5mm mono audio jack is recommended. 

## PWM Automation based on ambiental condition without temperature probes
I've removed the two temperature probe leads, as, based on personal experience, I rarely used them and they seemd hit or miss. Now, the automation of the dew heaters is done based on temperature and dew point. (Using the dedicated Control Center app, you can set three levels of agressivity for the PWM automation. In NINA/APT, this automation is fixed on the "Mid" level.

![image](https://github.com/user-attachments/assets/fa1a87fb-6252-4845-8d0e-0e9f8fe0fbea)


## Dedicated Control Center App
You can now control the power box through a dedicated app, built to interface with the ASCOM Driver. This allows you to use the power box without using NINA or other sequencer software. This could be useful in case you want to use planetary imaging software, like SharpCap, which, usually, do not have any Switch control module. Also, inside the Control Center, you can toggle the automatic PWM function for each output separately and you can control the aggresivity of the Auto PWM in 3 steps. 
![image](https://github.com/user-attachments/assets/e18eb556-9941-4ef8-9128-08519908c3b6)

## Ability to rename each port
When opening the Setup Dialog, you can rename any PWM, DC or EXT port, so you can keep track of which port each device is plugged in.

## Choose what happens when you disconnect from the device
Iside the Setup Dialog, you can choose to keep the ports on or turn them off when disconnecting. There are three options, one for the DC ports, one for the External ports and one for the PWM ports. 

## Carry over port state settings between control computers
On disconnect, the Arduino will store the ports state inside the EEPROM and will retreive them when the device is reinitiated. 

## Improved case design
The case has been redesigned for easier assembly and mounting options. It now has a 1/4 20 photo thread built into the base and the screws that keep the box closed have been now relocated to the top of the lid, instead of on its' side. The walls of the case have been improved, as well, now being thicker and stronger. The case now sports labels for each input and output and little holes for LEDs under each DC Port, to easily identify if power is applied to the port. 

## Improved power efficiency
The circuit has, also, been redesigned to accomodate more switches and, in doing so, the switching MOSFETs have been updated to a more efficient version. The IRF5305 MOSFETS used in the V2 box have a Drain to Source resistance of 60mOhms when fully on (Resistance provided by the datasheet, with Vgs = 10V). I've now switched to using SUD50P04 MOSFETS, which have only 7.5 - to 10 mOhms (provided by datasheet, measured at Vgs = 10V, with Id = 24A). This will ensure very low losses through the circuit, even at high current draw. Each MOSFET is capable of sustaining 40A of current, however, each DC port is limited by a resettable fuse to 5 Amps, which should be plenty for most astro equipment. There is also an internal, alwasy on, unfused 12V connector, in case you might want to extract a 12V supply inside the box, for a small step-up/step-down converter for other accesories. 

![20250525_155548](https://github.com/user-attachments/assets/b573c654-c1bf-4e05-91e8-2ceac6c227b1)


![20250525_155608](https://github.com/user-attachments/assets/c8e7a707-eaac-4a86-8880-c21b3bf1ad9d)


All the extra complexity has increased the power boxes' size, from 80 x 55 to 100 x 80, but it is still keeping a small enough size to be easily mounted on any telescope. 

NOTE: When building the box, you can add a capacitor between the RST and GND pins of the Arduino, in order to keep it from resetting when connecting to the computer. If you do this, when programming the device, you will need to press the Reset button on the arduino when the IDE will get to the "Uploading" step.


