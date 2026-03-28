using System;
using System.Collections.Generic;

namespace CarFactoryExample
{
    // Enum для типов машин
    public enum CarType
    {
        Tesla,
        BMW,
        Toyota
    }

    // Основной интерфейс автомобиля
    public interface ICar
    {
        string GetDescription();
    }

    // Интерфейсы для типа двигателя
    public interface IElectric { }
    public interface IMechanical { }

    // Интерфейсы для типа коробки передач
    public interface IAutomatic { }
    public interface IManual { }

    // Абстрактный класс автомобиля
    public abstract class ACar : ICar
    {
        public string Name { get; set; }
        public int Seats { get; set; }
        public string Dashboard { get; set; }

        public abstract string GetDescription();
    }

    // Абстрактный класс для электрических автомобилей
    public abstract class ElectricCar : ACar, IElectric
    {
        public override string GetDescription()
        {
            return $"{Name}: electrical car with {GetTransmission()}, {Seats} seats, {Dashboard} on board";
        }

        protected abstract string GetTransmission();
    }

    // Абстрактный класс для автомобилей с механикой
    public abstract class MechanicalCar : ACar, IMechanical
    {
        public override string GetDescription()
        {
            return $"{Name}: mechanical car with {GetTransmission()}, {Seats} seats, {Dashboard} on board";
        }

        protected abstract string GetTransmission();
    }

    // Конкретные автомобили
    public class Tesla : ElectricCar, IAutomatic
    {
        public Tesla()
        {
            Name = "Tesla";
            Seats = 5;
            Dashboard = "Android";
        }

        protected override string GetTransmission()
        {
            return "automatic transmission";
        }
    }

    public class BMW : MechanicalCar, IManual
    {
        public BMW()
        {
            Name = "BMW";
            Seats = 5;
            Dashboard = "iDrive";
        }

        protected override string GetTransmission()
        {
            return "manual transmission";
        }
    }

    public class Toyota : MechanicalCar, IAutomatic
    {
        public Toyota()
        {
            Name = "Toyota";
            Seats = 4;
            Dashboard = "Touchscreen";
        }

        protected override string GetTransmission()
        {
            return "automatic transmission";
        }
    }

    // Фабрика автомобилей
    public static class CarFactory
    {
        public static ICar CreateCar(CarType type)
        {
            return type switch
            {
                CarType.Tesla => new Tesla(),
                CarType.BMW => new BMW(),
                CarType.Toyota => new Toyota(),
                _ => throw new ArgumentException("Unknown car type")
            };
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var carNames = new Dictionary<string, CarType>(StringComparer.OrdinalIgnoreCase)
            {
                { "Tesla", CarType.Tesla },
                { "BMW", CarType.BMW },
                { "Toyota", CarType.Toyota }
            };

            while (true)
            {
                Console.Write("Введите марку автомобиля или done для остановки ввода: ");
                string input = Console.ReadLine();

                if (string.Equals(input, "done", StringComparison.OrdinalIgnoreCase))
                    break;

                if (carNames.ContainsKey(input))
                {
                    ICar car = CarFactory.CreateCar(carNames[input]);
                    Console.WriteLine(car.GetDescription());
                }
                else
                {
                    Console.WriteLine("Марка автомобиля не найдена.");
                }
            }
        }
    }
}
