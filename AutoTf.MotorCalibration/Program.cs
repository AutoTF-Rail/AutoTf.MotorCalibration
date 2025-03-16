using System.Device.I2c;
using Iot.Device.Pwm;

internal class Program
{
	private const int PWM_FREQUENCY = 50;      // Frequency for servo motors (50 Hz)
	private const double MIN_PULSE_WIDTH_MS = 0.5;  // Minimum pulse width for servo (ms)
	private const double MAX_PULSE_WIDTH_MS = 2.5;
	private const double NEUTRAL_PULSE_WIDTH_MS = 1.5;
	
	public static void Main(string[] args)
	{
		I2cConnectionSettings i2CSettings = new I2cConnectionSettings(1, Pca9685.I2cAddressBase);
		I2cDevice i2CDevice = I2cDevice.Create(i2CSettings);
		byte readValue = i2CDevice.ReadByte();
		
		
		ResetPca9685(i2CDevice);
		
		Pca9685 pca9685 = new Pca9685(i2CDevice);
		Console.WriteLine("Cycle: " + pca9685.GetDutyCycle(10));
		
		void SetServoAngle(int lever, int angle, int offset = 0)
        {
            angle += offset;
            angle = Math.Max(0, Math.Min(270, angle));

            double pulseWidthMs = MIN_PULSE_WIDTH_MS + (angle / 270.0) * (MAX_PULSE_WIDTH_MS - MIN_PULSE_WIDTH_MS);
            double dutyCycle = (pulseWidthMs / (1000.0 / PWM_FREQUENCY)) * 100;
            pca9685.SetDutyCycle(lever, dutyCycle / 100);
        }

		void MoveToNeutralPosition()
		{
			double dutyCycle = (NEUTRAL_PULSE_WIDTH_MS / (1000.0 / PWM_FREQUENCY)) * 100;
			pca9685.SetDutyCycle(0, dutyCycle / 100);
			pca9685.SetDutyCycle(1, dutyCycle / 100);
		}
		
		MoveToNeutralPosition();
		Thread.Sleep(1250);
		bool canRun = true;
		
		while(canRun)
		{
			Console.Clear();
			Console.WriteLine("Ready for input:");
			Console.WriteLine("[1] Move to 135deg");
			Console.WriteLine("[2] Demo angles");
			Console.WriteLine("[3] Demo two levers");
			Console.WriteLine("[4] Set Deg");
			Console.WriteLine("[5] Exit");
			string input = Console.ReadLine()!;
			if (input == "1")
			{
				SetServoAngle(0, 135);
				SetServoAngle(1, 135, 5);
				Thread.Sleep(1500);
			}
			else if(input == "2")
			{
				for (int i = 0; i < 150; i++)
				{
					Console.WriteLine($"[{i}]");
					Console.WriteLine("Moving to 135 degrees");
					SetServoAngle(0, 135, 5);
					Thread.Sleep(1000);
					Console.WriteLine("Moving to 135 + 80 degrees");
					SetServoAngle(0, 135 + 80, 0);
					Thread.Sleep(1000);
					Console.WriteLine("Moving to (135 - 60 degrees");
					SetServoAngle(0, 135 - 70, 0);
					Thread.Sleep(1250);
				}
			}
			else if(input == "3")
			{
				for (int i = 0; i < 150; i++)
				{
					Console.WriteLine($"[{i}]");
					
					SetServoAngle(0, 135, 0);
					SetServoAngle(1, 135, 0);
					Thread.Sleep(1000);
					
					SetServoAngle(0, 135 + 45, 0);
					SetServoAngle(1, 135 - 45, 0);
					Thread.Sleep(1000);
					
					SetServoAngle(0, 135 - 45, 0);
					SetServoAngle(1, 135 + 45, 0);
					Thread.Sleep(1200);
				}
			}
			else if (input == "4")
			{
				bool canRunInner = true;
				while (canRunInner)
				{
					Console.Write("Lever: ");
					string? lever = Console.ReadLine();
					if (!int.TryParse(lever, out int leverInt))
						return;
					Console.Write("Degree: ");
					string? inputNew = Console.ReadLine();
					if (inputNew!.ToLower() == "e")
						canRunInner = false;
					else if(int.TryParse(inputNew, out int result))
					{
						int offset = 0;
						// if(leverInt == 1)
						//  offset = 5;
						SetServoAngle(leverInt, result, offset);
					}
				}
			}
			else
			{
				canRun = false;
			}
		}
		MoveToNeutralPosition();
		// pwmChannel.Dispose();
		pca9685.Dispose();
		i2CDevice.Dispose();
	}
	
	private static void ResetPca9685(I2cDevice i2CDevice)
	{
		byte mode1RegisterAddress = 0x00;
		byte resetValue = 0x80; 
        
		i2CDevice.Write(new ReadOnlySpan<byte>(new byte[] { mode1RegisterAddress, resetValue }));

		Thread.Sleep(10);
	}
}