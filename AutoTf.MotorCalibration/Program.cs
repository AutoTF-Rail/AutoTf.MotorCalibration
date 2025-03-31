using System.Device.I2c;
using Iot.Device.Pwm;
// ReSharper disable AccessToDisposedClosure

namespace AutoTf.MotorCalibration;

internal static class Program
{
	public static void Main(string[] args)
	{
		I2cConnectionSettings i2CSettings = new I2cConnectionSettings(1, 0x40);
		I2cDevice i2CDevice = I2cDevice.Create(i2CSettings);
		
		ResetPca9685(i2CDevice);
		
		Pca9685 pca = new Pca9685(i2CDevice);

		Thread.Sleep(1250);
		bool canRun = true;
		
		while(canRun)
		{
			Console.Clear();
			Console.WriteLine("[1] Move to 135deg");
			Console.WriteLine("[2] Demo two levers");
			Console.WriteLine("[3] Set Deg");
			Console.WriteLine("[4] Set Duty Cycle");
			Console.WriteLine("[5] Exit");
			
			string input = Console.ReadLine()!;
			
			switch (input)
			{
				case "1":
					SetServoToCenter(pca);
					break;
				case "2":
					Demo2LeverAngles(pca);
					break;
				case "3":
					SetDegLoop(pca);
					break;
				case "4":
					SetDutyCycleLoop(pca);
					break;
				default:
					canRun = false;
					break;
			}
		}
		
		pca.Dispose();
		i2CDevice.Dispose();
	}

	private static void SetServoToCenter(Pca9685 pca)
	{
		SetServoAngle(pca, 0, 135);
		SetServoAngle(pca, 1, 135, 5);
	}

	private static void SetDutyCycleLoop(Pca9685 pca)
	{
		bool canRunInner = true;
		while (canRunInner)
		{
			Console.Write("Lever: ");
			string? lever = Console.ReadLine();
			
			if (!int.TryParse(lever, out int leverInt))
				continue;
			
			Console.Write("Degree: ");
			string? inputNew = Console.ReadLine();
			
			if (inputNew!.ToLower() == "e")
				canRunInner = false;
			else if(double.TryParse(inputNew, out double result))
			{
				pca.SetDutyCycle(leverInt, result);
			}
		}
	}

	private static void SetDegLoop(Pca9685 pca)
	{
		bool canRunInner = true;
		while (canRunInner)
		{
			Console.Write("Lever: ");
			string? lever = Console.ReadLine();
					
			if (!int.TryParse(lever, out int leverInt))
				continue;
					
			Console.Write("Degree: ");
			string? inputNew = Console.ReadLine();
					
			if (inputNew!.ToLower() == "e")
				canRunInner = false;
			else if(int.TryParse(inputNew, out int result))
			{
				SetServoAngle(pca, leverInt, result);
			}
		}
	}

	private static void Demo2LeverAngles(Pca9685 pca)
	{
		for (int i = 0; i < 150; i++)
		{
			Console.WriteLine($"[{i}]");
					
			Task.Run(() => SetServoAngle(pca, 0, 135));
			Task.Run(() => SetServoAngle(pca, 1, 135));
			Thread.Sleep(600);
					
			Task.Run(() => SetServoAngle(pca, 0, 135 + 45));
			Task.Run(() => SetServoAngle(pca, 1, 135 - 45));
			Thread.Sleep(600);
					
			Task.Run(() => SetServoAngle(pca, 0, 135 - 45));
			Task.Run(() => SetServoAngle(pca, 1, 135 + 45));
			Thread.Sleep(800);
		}
	}

	private static void SetServoAngle(Pca9685 pca, int lever, int angle, int offset = 0)
	{
		angle += offset;
		angle = Math.Max(0, Math.Min(270, angle));

		double minDutyCycle = 0.065;
		double maxDutyCycle = 0.53;

		double dutyCycle = minDutyCycle + (angle / 270.0) * (maxDutyCycle - minDutyCycle);

		pca.SetDutyCycle(lever, dutyCycle);
	}

	/// <summary>
	/// This method is needed, to soft reset the PCA board. If we don't do this on startup, it might freeze and we won't be able to control the servos.
	/// </summary>
	private static void ResetPca9685(I2cDevice i2CDevice)
	{
		byte mode1RegisterAddress = 0x00;
		byte resetValue = 0x80; 
        
		i2CDevice.Write(new ReadOnlySpan<byte>([mode1RegisterAddress, resetValue]));

		Thread.Sleep(10);
	}
}