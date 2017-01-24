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
                if (item == newItem)
                {
                    Vehicle car = LocalPlayer.Character.CurrentVehicle;
                    if (car != null)
                    {
                        ToggleEngine(car);
                    }
                    else if (savedVehicle != null)
                    {
                        ToggleEngine(savedVehicle);
                    }
                    else
                    {
                        Screen.ShowNotification(ERROR_NOCAR);
                    }
                }
            };
        }

        private void ToggleEngine(Vehicle car)
        {
            if (car.IsEngineRunning)
            {
                Screen.ShowNotification("Engine is now ~r~off~w~.");
                car.FuelLevel = 0;
                car.IsEngineRunning = false;
            }
            else
            {
                Screen.ShowNotification("Engine is now ~g~on~w~.");
                car.FuelLevel = 65f;
                car.IsEngineRunning = true;
            }
        }

        private void AddDoorLockItem(UIMenu menu)
        {
            var newItem = new UIMenuItem("Toggle Door Lock");
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == newItem)
                {
                    Vehicle car = LocalPlayer.Character.CurrentVehicle;
                    if (car != null)
                    {
                        LockDoor(car);
                    }
                    else if (savedVehicle != null)
                    {
                        LockDoor(savedVehicle);
                    }
                    else
                    {
                        Screen.ShowNotification(ERROR_NOCAR);
                    }
                }
            };
        }

        private void LockDoor(Vehicle car)
        {
            if (Function.Call<int>(Hash.GET_VEHICLE_DOOR_LOCK_STATUS, car) != 2)
            {
                Screen.ShowNotification("Doors are ~r~locked~w~.");
                Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED, car, 2);
            }
            else
            {
                Screen.ShowNotification("Doors are ~g~unlocked~w~.");
                Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED, car, 0);
            }
        }

        private void AddOpenDoorItem(UIMenu menu)
        {
            var doors = new List<dynamic>
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
                if (item == newItem)
                {
                    int itemIndex = newItem.Index;
                    string doorName = newItem.IndexToItem(itemIndex);
                    Vehicle car = LocalPlayer.Character.CurrentVehicle;
                    if (car != null)
                    {
                        ToggleDoor(car, itemIndex, doorName);
                    }
                    else if (savedVehicle != null)
                    {
                        ToggleDoor(savedVehicle, itemIndex, doorName);
                    }
                    else
                    {
                        Screen.ShowNotification(ERROR_NOCAR);
                    }
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
            if (doorAngle == 0)
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

        private void AddSaveVehicleItem(UIMenu menu)
        {
            var newItem = new UIMenuItem("Save vehicle");
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == newItem)
                {
                    Vehicle car = LocalPlayer.Character.CurrentVehicle;
                    if (car == null)
                    {
                        Screen.ShowNotification(ERROR_NOCAR);
                        return;
                    }
                    else if (savedVehicle != null)
                    {
                        savedVehicle.AttachedBlip.Alpha = 0;
                    }

                    newItem.SetRightLabel(car.LocalizedName);
                    SaveVehicle(car);
                }
            };
        }

        private void SaveVehicle(Vehicle car)
        {
            Screen.ShowNotification("Saved vehicle.");
            if (car.AttachedBlip == null)
            {
                Blip blip = car.AttachBlip();
                blip.Sprite = BlipSprite.PersonalVehicleCar;
            }

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
