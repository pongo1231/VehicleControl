using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VehicleControls
{
    public class VehicleControls : BaseScript
    {
        private static string ERROR = "~r~Error: ";
        private static string ERROR_NOCAR = ERROR + "You aren't in a vehicle nor do you have a saved vehicle.";

        private Vehicle savedVehicle;

        private void AddEngineItem(UIMenu menu)
        {
            var newItem = new UIMenuItem("Toggle Engine");
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item != newItem)
                {
                    return;
                }

                Vehicle car = LocalPlayer.Character.CurrentVehicle;

                if (car == null
                 && savedVehicle == null)
                {
                    Screen.ShowNotification(ERROR_NOCAR);
                    return;
                }

                if (car != null)
                {
                    ToggleEngine(car);
                }
                else if (savedVehicle != null)
                {
                    ToggleEngine(savedVehicle);
                }
            };
        }

        private void ToggleEngine(Vehicle car)
        {
            if (car.IsEngineRunning)
            {
                Screen.ShowNotification("Engine is now ~r~off~w~.");
                car.IsDriveable = false;
                car.IsEngineRunning = false;
            }
            else
            {
                Screen.ShowNotification("Engine is now ~g~on~w~.");
                car.IsDriveable = true;
                car.IsEngineRunning = true;
            }
        }

        private void AddDoorLockItem(UIMenu menu)
        {
            var newItem = new UIMenuItem("Toggle Door Lock", "NOTE: This will also set this vehicle as saved vehicle.");
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == newItem)
                {
                    Vehicle car = LocalPlayer.Character.CurrentVehicle;

                    if (car == null
                     && savedVehicle == null)
                    {
                        Screen.ShowNotification(ERROR_NOCAR);
                        return;
                    }

                    if (car != null)
                    {
                        LockDoor(car);
                        SaveVehicle(car);
                    }
                    else
                    {
                        LockDoor(savedVehicle);
                    }
                }
            };
        }

        private void LockDoor(Vehicle car)
        {
            bool doorLocked = (Function.Call<int>(Hash.GET_VEHICLE_DOOR_LOCK_STATUS, car) == 2);

            if (doorLocked)
            {
                Screen.ShowNotification("Doors are ~g~unlocked~w~.");
                Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED, car, 0);
            }
            else
            {
                Screen.ShowNotification("Doors are ~r~locked~w~.");
                Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED, car, 2);
            }
        }

        private void AddOpenDoorItem(UIMenu menu)
        {
            List<dynamic> doors = new List<dynamic>
            {
                "Front Left",
                "Front Right",
                "Back Left",
                "Back Right",
                "Hood",
                "Trunk"
            };
            var newItem = new UIMenuListItem("Toggle Door", doors, 0);
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item != newItem)
                {
                    return;
                }

                int itemIndex = newItem.Index;
                string doorName = newItem.IndexToItem(itemIndex);
                Vehicle car = LocalPlayer.Character.CurrentVehicle;

                if (car == null
                 && savedVehicle == null)
                {
                    Screen.ShowNotification(ERROR_NOCAR);
                    return;
                }

                if (car != null)
                {
                    ToggleDoor(car, itemIndex, doorName);
                }
                else
                {
                    ToggleDoor(savedVehicle, itemIndex, doorName);
                }
            };
        }

        private void ToggleDoor(Vehicle car, int index, string doorName)
        {
            bool doorBroken = Function.Call<bool>(Hash.IS_VEHICLE_DOOR_DAMAGED, car, index);
            if (doorBroken)
            {
                Screen.ShowNotification(ERROR + "Door is broken.");
                return;
            }

            float doorAngle = Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, car, index);
            if (doorAngle == 0) // Door is closed
            {
                Screen.ShowNotification(doorName + " Door is now ~g~open~w~.");
                Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, car, index, false, false);
            }
            else
            {
                Screen.ShowNotification(doorName + " Door is now ~r~shut~w~.");
                Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, car, index, false);
            }
        }

        private void AddLockSpeedItem(UIMenu menu)
        {
            List<dynamic> speeds = new List<dynamic>()
            {
                "None"
            };
            for (int i = 30; i < 121; i = i + 10)
            {
                speeds.Add(i + " KM/H");
            }
            UIMenuListItem newItem = new UIMenuListItem("Lock Max Speed", speeds, 0);
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item != newItem)
                {
                    return;
                }

                Vehicle car = LocalPlayer.Character.CurrentVehicle;

                if (car == null
                 && savedVehicle == null)
                {
                    Screen.ShowNotification(ERROR_NOCAR);
                    return;
                }

                if (car != null)
                {
                    LockSpeed(car, newItem);
                }
                else
                {
                    LockSpeed(savedVehicle, newItem);
                }
            };
        }

        private void LockSpeed(Vehicle car, UIMenuListItem item)
        {
            string[] itemName = item.IndexToItem(item.Index).Split(' ');
            if (itemName[0] == "None")
            {
                car.MaxSpeed = int.MaxValue;
                Screen.ShowNotification($"Speedlimit has been removed.");
                return;
            }

            float itemSpeed = float.Parse(itemName[0]) / 3.6f;
            car.MaxSpeed = itemSpeed;
            Screen.ShowNotification($"Speed has been limited to {itemName[0]} {itemName[1]}.");
        }

        private void AddSaveVehicleItem(UIMenu menu)
        {
            var newItem = new UIMenuItem("Save vehicle");
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item != newItem)
                {
                    return;
                }

                Vehicle car = LocalPlayer.Character.CurrentVehicle;

                if (car == null)
                {
                    Screen.ShowNotification(ERROR_NOCAR);
                    return;
                }

                SaveVehicle(car);
                Screen.ShowNotification("Saved vehicle.");
            };
        }

        private void SaveVehicle(Vehicle car)
        {
            if (savedVehicle != null)
            {
                foreach (Blip vehBlip in savedVehicle.AttachedBlips)
                {
                    vehBlip.Alpha = 0;
                }
            }

            Blip blip = car.AttachBlip();
            blip.Sprite = BlipSprite.PersonalVehicleCar;

            savedVehicle = car;
        }

        public VehicleControls()
        {
            MenuPool menuPool = new MenuPool();

            UIMenu menu = new UIMenu("Vehicle Controls", "");
            menuPool.Add(menu);

            AddEngineItem(menu);
            AddDoorLockItem(menu);
            AddOpenDoorItem(menu);
            AddLockSpeedItem(menu);
            AddSaveVehicleItem(menu);

            menu.RefreshIndex();

            Tick += new Func<Task>(async delegate
            {
                await Task.FromResult(0);

                menuPool.ProcessMenus();
                if (Game.IsControlJustReleased(1, Control.InteractionMenu))
                {
                    menu.Visible = !menu.Visible;
                }
            });
        }
    }
}
