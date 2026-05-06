using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using FermixAPI.Core;
using UnityEngine;
using Camera = Exiled.API.Features.Camera;

namespace FermixAPI.Systems
{
    /// <summary>
    /// Система управления камерами и наблюдением.
    /// </summary>
    public static class FermixCams
    {
        #region Camera Queries - Поиск Камер

        /// <summary>
        /// Получает все камеры.
        /// </summary>
        public static IEnumerable<Camera> GetAll()
        {
            return Camera.List;
        }

        /// <summary>
        /// Получает количество камер.
        /// </summary>
        public static int Count()
        {
            return Camera.List.Count;
        }

        /// <summary>
        /// Получает камеры в зоне.
        /// </summary>
        public static IEnumerable<Camera> GetInZone(ZoneType zone)
        {
            return Camera.List.Where(c => c.Zone == zone);
        }

        /// <summary>
        /// Получает ближайшую камеру к позиции.
        /// </summary>
        public static Camera GetNearest(Vector3 position)
        {
            return Camera.List
                .OrderBy(c => Vector3.Distance(c.Position, position))
                .FirstOrDefault();
        }

        /// <summary>
        /// Получает ближайшую камеру к игроку.
        /// </summary>
        public static Camera GetNearest(Player player)
        {
            return GetNearest(player.Position);
        }

        /// <summary>
        /// Получает случайную камеру.
        /// </summary>
        public static Camera GetRandom()
        {
            var cameras = Camera.List.ToList();
            return cameras.Count > 0 ? cameras[UnityEngine.Random.Range(0, cameras.Count)] : null;
        }

        /// <summary>
        /// Получает случайную камеру в зоне.
        /// </summary>
        public static Camera GetRandom(ZoneType zone)
        {
            var cameras = GetInZone(zone).ToList();
            return cameras.Count > 0 ? cameras[UnityEngine.Random.Range(0, cameras.Count)] : null;
        }

        #endregion

        #region Camera State - Состояние Камер

        /// <summary>
        /// Проверяет, используется ли хоть одна камера.
        /// </summary>
        public static bool IsAnyCameraInUse()
        {
            return Camera.List.Any(c => c.IsBeingUsed);
        }

        /// <summary>
        /// Получает все используемые камеры.
        /// </summary>
        public static IEnumerable<Camera> GetInUse()
        {
            return Camera.List.Where(c => c.IsBeingUsed);
        }

        /// <summary>
        /// Получает количество используемых камер.
        /// </summary>
        public static int CountInUse()
        {
            return GetInUse().Count();
        }

        /// <summary>
        /// Проверяет, наблюдают ли за комнатой.
        /// </summary>
        public static bool IsRoomBeingWatched(Room room)
        {
            return Camera.List
                .Where(c => c.Room == room)
                .Any(c => c.IsBeingUsed);
        }

        /// <summary>
        /// Проверяет, наблюдают ли за зоной.
        /// </summary>
        public static bool IsZoneBeingWatched(ZoneType zone)
        {
            return GetInZone(zone).Any(c => c.IsBeingUsed);
        }

        #endregion

        #region SCP-079 Operations - Операции SCP-079

        /// <summary>
        /// Получает игрока SCP-079.
        /// </summary>
        public static Player Get079()
        {
            return Player.List.FirstOrDefault(p => p.Role.Type == PlayerRoles.RoleTypeId.Scp079);
        }

        /// <summary>
        /// Получает камеру, на которую смотрит SCP-079.
        /// </summary>
        public static Camera Get079CurrentCamera()
        {
            var scp079 = Get079();
            if (scp079?.Role is Exiled.API.Features.Roles.Scp079Role role)
            {
                return role.Camera;
            }
            return null;
        }

        /// <summary>
        /// Переключает SCP-079 на камеру.
        /// </summary>
        public static void Switch079ToCamera(Camera camera)
        {
            var scp079 = Get079();
            if (scp079?.Role is Exiled.API.Features.Roles.Scp079Role role)
            {
                role.Camera = camera;
            }
        }

        /// <summary>
        /// Переключает SCP-079 на случайную камеру.
        /// </summary>
        public static void Switch079ToRandom()
        {
            var camera = GetRandom();
            if (camera != null)
            {
                Switch079ToCamera(camera);
            }
        }

        /// <summary>
        /// Переключает SCP-079 на случайную камеру в зоне.
        /// </summary>
        public static void Switch079ToRandomInZone(ZoneType zone)
        {
            var camera = GetRandom(zone);
            if (camera != null)
            {
                Switch079ToCamera(camera);
            }
        }

        #endregion

        #region Camera Users - Пользователи Камер

        /// <summary>
        /// Ослепляет всех, кто смотрит в камеры.
        /// </summary>
        public static void FlashCameraUsers(float duration = 3f, byte intensity = 1)
        {
            var scp079 = Get079();
            if (scp079 != null)
            {
                scp079.EnableEffect(EffectType.Blinded, intensity, duration);
            }
            FermixLog.Action($"Пользователи камер ослеплены на {duration} сек");
        }

        /// <summary>
        /// Наносит урон пользователям камер.
        /// </summary>
        public static void DamageCameraUsers(float damage)
        {
            var scp079 = Get079();
            if (scp079 != null)
            {
                scp079.Hurt(damage, "Camera System Overload");
            }
        }

        /// <summary>
        /// Отключает SCP-079 от камер на время.
        /// </summary>
        public static void DisruptCameras(float duration = 5f)
        {
            var scp079 = Get079();
            if (scp079?.Role is Exiled.API.Features.Roles.Scp079Role role)
            {
                // Сохраняем энергию и устанавливаем в 0
                var energy = role.Energy;
                role.Energy = 0;
                
                FermixScheduler.Delay(duration, () =>
                {
                    // Игрок мог умереть или сменить роль за время задержки —
                    // повторно резолвим Scp079Role, чтобы не писать в stale-ссылку.
                    if (scp079 != null && scp079.IsConnected &&
                        scp079.Role is Exiled.API.Features.Roles.Scp079Role currentRole)
                    {
                        currentRole.Energy = energy;
                    }
                });
                
                FermixLog.Action($"Камеры нарушены на {duration} сек");
            }
        }

        #endregion

        #region Intercom Operations - Операции с Интеркомом

        /// <summary>
        /// Получает текущего пользователя интеркома.
        /// </summary>
        public static Player GetIntercomUser()
        {
            return Intercom.Speaker;
        }

        /// <summary>
        /// Проверяет, используется ли интерком.
        /// </summary>
        public static bool IsIntercomInUse()
        {
            return Intercom.Speaker != null;
        }

        /// <summary>
        /// Устанавливает кулдаун интеркома.
        /// </summary>
        public static void SetIntercomCooldown(float seconds)
        {
            Intercom.RemainingCooldown = seconds;
        }

        /// <summary>
        /// Сбрасывает кулдаун интеркома.
        /// </summary>
        public static void ResetIntercomCooldown()
        {
            Intercom.RemainingCooldown = 0f;
        }

        /// <summary>
        /// Устанавливает время речи интеркома.
        /// </summary>
        public static void SetIntercomSpeechTime(float seconds)
        {
            Intercom.SpeechRemainingTime = seconds;
        }

        /// <summary>
        /// Устанавливает текст интеркома.
        /// </summary>
        public static void SetIntercomText(string text)
        {
            Intercom.DisplayText = text;
        }

        /// <summary>
        /// Очищает текст интеркома.
        /// </summary>
        public static void ClearIntercomText()
        {
            Intercom.DisplayText = string.Empty;
        }

        #endregion

        #region Surveillance Effects - Эффекты Наблюдения

        /// <summary>
        /// Создаёт помехи на камерах (эффект для 079).
        /// </summary>
        public static void CreateStatic(float duration = 3f)
        {
            var scp079 = Get079();
            if (scp079 != null)
            {
                scp079.EnableEffect(EffectType.Flashed, 1, duration);
            }
        }

        /// <summary>
        /// Отслеживает игрока через камеры.
        /// </summary>
        public static void TrackPlayer(Player target, float interval = 2f, float duration = 30f)
        {
            FermixCore.RunCoroutine(TrackPlayerCoroutine(target, interval, duration));
        }

        private static IEnumerator<float> TrackPlayerCoroutine(Player target, float interval, float duration)
        {
            float elapsed = 0f;
            
            while (elapsed < duration && target != null && target.IsConnected && target.IsAlive)
            {
                var nearestCamera = GetNearest(target);
                if (nearestCamera != null)
                {
                    Switch079ToCamera(nearestCamera);
                }
                
                yield return MEC.Timing.WaitForSeconds(interval);
                elapsed += interval;
            }
        }

        /// <summary>
        /// Автоматическое переключение камер.
        /// </summary>
        public static void AutoSwitch(float interval = 5f, int count = 10)
        {
            FermixCore.RunCoroutine(AutoSwitchCoroutine(interval, count));
        }

        private static IEnumerator<float> AutoSwitchCoroutine(float interval, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Switch079ToRandom();
                yield return MEC.Timing.WaitForSeconds(interval);
            }
        }

        #endregion

        #region Camera Extensions - Расширения Камер

        /// <summary>
        /// Расстояние от камеры до позиции.
        /// </summary>
        public static float DistanceTo(this Camera camera, Vector3 position)
        {
            return Vector3.Distance(camera.Position, position);
        }

        /// <summary>
        /// Расстояние от камеры до игрока.
        /// </summary>
        public static float DistanceTo(this Camera camera, Player player)
        {
            return camera.DistanceTo(player.Position);
        }

        /// <summary>
        /// Получает игроков в радиусе камеры.
        /// </summary>
        public static IEnumerable<Player> GetPlayersInRange(this Camera camera, float radius = 20f)
        {
            return Player.List.Where(p => camera.DistanceTo(p) <= radius && p.IsAlive);
        }

        /// <summary>
        /// Проверяет, виден ли игрок с камеры.
        /// </summary>
        public static bool CanSee(this Camera camera, Player player, float maxDistance = 30f)
        {
            return camera.DistanceTo(player) <= maxDistance;
        }

        #endregion
    }
}
