# AutoTf.MotorCalibration


A very simple CLI tool to control and calibrate (set desired angles) motors used in AutoTF products, via a PCA9685.

The calculation code in this repo is also used in [AutoTF-Rail/AutoTf.CentralBridgeOS](https://github.com/AutoTF-Rail/AutoTf.CentralBridgeOS) > [MotorManager.cs](https://github.com/AutoTF-Rail/AutoTf.CentralBridgeOS/blob/main/AutoTf.CentralBridgeOS.Services/MotorManager.cs)

## Pin configuration

The code in this repo is being run on a Raspberry Pi 4 Model B, using the following pin configuration to the BCA9685:
(with the pins of the PI on the right side view, top to bottom):
1: EMPTY 2: VCC
3: SDA 4: V+
5: SCL 6: GND


## Info & Contributions

Further documentation can be seen in [AutoTF-Rail/AutoTf-Documentation](https://github.com/AutoTF-Rail/AutoTf-Documentation)


Would you like to contribute to this project, or noticed something wrong?

Feel free to contact us at [opensource@autotf.de](mailto:opensource@autotf.de)
