using System.Device.I2c;
using Iot.Device.Pwm;

namespace AutoTf.MotorCalibration;

internal static class Program
{
	public static void Main(string[] args)
	{
		I2cConnectionSettings i2CSettings = new I2cConnectionSettings(1, 0x40);
		I2cDevice i2CDevice = I2cDevice.Create(i2CSettings);
		
		ResetPca9685(i2CDevice);
		
		Pca9685 pca9685 = new Pca9685(i2CDevice);
		Console.WriteLine("Cycle: " + pca9685.GetDutyCycle(1));
		
		void SetServoAngle(int lever, int angle, int offset = 0)
		{
			angle += offset;
			angle = Math.Max(0, Math.Min(270, angle));

			double minDutyCycle = 0.065;
			double maxDutyCycle = 0.53;

			double dutyCycle = minDutyCycle + (angle / 270.0) * (maxDutyCycle - minDutyCycle);

			pca9685.SetDutyCycle(lever, dutyCycle);
		}

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
			Console.WriteLine("[5] Set Duty Cycle");
			Console.WriteLine("[6] Exit");
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
					SetServoAngle(0, 135 + 80);
					Thread.Sleep(1000);
					
					Console.WriteLine("Moving to (135 - 70 degrees");
					SetServoAngle(0, 135 - 70);
					Thread.Sleep(1250);
				}
			}
			else if(input == "3")
			{
				for (int i = 0; i < 150; i++)
				{
					Console.WriteLine($"[{i}]");
					
					Task.Run(() => SetServoAngle(0, 135));
					Task.Run(() => SetServoAngle(1, 135));
					Thread.Sleep(600);
					
					Task.Run(() => SetServoAngle(0, 135 + 45));
					Task.Run(() => SetServoAngle(1, 135 - 45));
					Thread.Sleep(600);
					
					Task.Run(() => SetServoAngle(0, 135 - 45));
					Task.Run(() => SetServoAngle(1, 135 + 45));
					Thread.Sleep(800);
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
						SetServoAngle(leverInt, result);
					}
				}
			}
			else if (input == "5")
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
					else if(double.TryParse(inputNew, out double result))
					{
						pca9685.SetDutyCycle(leverInt, result);
					}
				}
			}
			else
			{
				canRun = false;
			}
		}
		
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