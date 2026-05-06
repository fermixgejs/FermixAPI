using System;
using Exiled.API.Features;
using FermixAPI.Core;
using MEC;
using System.Collections.Generic;

namespace FermixAPI.Systems
{
    /// <summary>
    /// Расширенное управление боеголовкой.
    /// </summary>
    public static class FermixWarhead
    {
        #region Basic Controls - Базовое Управление

        /// <summary>
        /// Запускает боеголовку.
        /// </summary>
        public static void Start()
        {
            Warhead.Start();
            FermixLog.Action("Боеголовка запущена");
        }

        /// <summary>
        /// Останавливает боеголовку.
        /// </summary>
        public static void Stop()
        {
            Warhead.Stop();
            FermixLog.Action("Боеголовка остановлена");
        }

        /// <summary>
        /// Мгновенный взрыв боеголовки.
        /// </summary>
        public static void Detonate()
        {
            Warhead.Detonate();
            FermixLog.Action("Боеголовка взорвана");
        }

        /// <summary>
        /// Сбрасывает боеголовку в начальное состояние.
        /// </summary>
        public static void Reset()
        {
            Warhead.Stop();
            Warhead.DetonationTimer = 90f;
            Warhead.IsLocked = false;
            FermixLog.Action("Боеголовка сброшена");
        }

        #endregion

        #region Timer Control - Управление Таймером

        /// <summary>
        /// Устанавливает время до взрыва.
        /// </summary>
        public static void SetTime(float seconds)
        {
            Warhead.DetonationTimer = seconds;
            FermixLog.Action($"Таймер боеголовки: {seconds} сек");
        }

        /// <summary>
        /// Получает оставшееся время до взрыва.
        /// </summary>
        public static float GetTime()
        {
            return Warhead.DetonationTimer;
        }

        /// <summary>
        /// Добавляет время к таймеру.
        /// </summary>
        public static void AddTime(float seconds)
        {
            Warhead.DetonationTimer += seconds;
            FermixLog.Action($"Добавлено {seconds} сек к таймеру");
        }

        /// <summary>
        /// Уменьшает время таймера.
        /// </summary>
        public static void SubtractTime(float seconds)
        {
            Warhead.DetonationTimer = Math.Max(0, Warhead.DetonationTimer - seconds);
        }

        /// <summary>
        /// Запускает боеголовку с указанным временем.
        /// </summary>
        public static void StartWithTime(float seconds)
        {
            SetTime(seconds);
            Start();
        }

        /// <summary>
        /// Запускает боеголовку с мгновенным взрывом через указанное время.
        /// </summary>
        public static void InstantIn(float seconds)
        {
            FermixScheduler.Delay(seconds, Detonate);
            FermixLog.Warn($"Боеголовка взорвётся через {seconds} секунд!");
        }

        #endregion

        #region Lock Control - Управление Блокировкой

        /// <summary>
        /// Блокирует боеголовку.
        /// </summary>
        public static void Lock()
        {
            Warhead.IsLocked = true;
            FermixLog.Action("Боеголовка заблокирована");
        }

        /// <summary>
        /// Разблокирует боеголовку.
        /// </summary>
        public static void Unlock()
        {
            Warhead.IsLocked = false;
            FermixLog.Action("Боеголовка разблокирована");
        }

        /// <summary>
        /// Переключает блокировку боеголовки.
        /// </summary>
        public static void ToggleLock()
        {
            Warhead.IsLocked = !Warhead.IsLocked;
            FermixLog.Action($"Блокировка боеголовки: {(Warhead.IsLocked ? "ВКЛ" : "ВЫКЛ")}");
        }

        /// <summary>
        /// Блокирует боеголовку на время.
        /// </summary>
        public static void LockFor(float seconds)
        {
            Lock();
            FermixScheduler.Delay(seconds, Unlock);
        }

        #endregion

        #region State Checks - Проверки Состояния

        /// <summary>
        /// Проверяет, запущена ли боеголовка.
        /// </summary>
        public static bool IsInProgress => Warhead.IsInProgress;

        /// <summary>
        /// Проверяет, взорвалась ли боеголовка.
        /// </summary>
        public static bool IsDetonated => Warhead.IsDetonated;

        /// <summary>
        /// Проверяет, заблокирована ли боеголовка.
        /// </summary>
        public static bool IsLocked => Warhead.IsLocked;

        /// <summary>
        /// Проверяет, можно ли отменить взрыв.
        /// </summary>
        public static bool CanBeStopped => Warhead.IsInProgress && !Warhead.IsLocked && Warhead.DetonationTimer > 10f;

        /// <summary>
        /// Время до взрыва.
        /// </summary>
        public static float TimeLeft => Warhead.DetonationTimer;

        #endregion

        #region Events & Callbacks - События и Колбэки

        private static CoroutineHandle _tickerHandle;
        private static Action<float> _onTick;
        private static Action _onDetonate;

        /// <summary>
        /// Запускает боеголовку с callback на каждую секунду.
        /// </summary>
        public static void StartWithCallback(Action<float> onTick, Action onDetonate = null)
        {
            _onTick = onTick;
            _onDetonate = onDetonate;
            
            Start();
            _tickerHandle = FermixCore.RunCoroutine(TickCoroutine());
        }

        private static IEnumerator<float> TickCoroutine()
        {
            while (IsInProgress && !IsDetonated)
            {
                _onTick?.Invoke(TimeLeft);
                yield return Timing.WaitForSeconds(1f);
            }
            
            if (IsDetonated)
            {
                _onDetonate?.Invoke();
            }
        }

        /// <summary>
        /// Останавливает отслеживание таймера.
        /// </summary>
        public static void StopCallback()
        {
            if (_tickerHandle.IsRunning)
            {
                Timing.KillCoroutines(_tickerHandle);
            }
            _onTick = null;
            _onDetonate = null;
        }

        /// <summary>
        /// Выполняет действие при достижении определённого времени.
        /// </summary>
        public static void OnTimeReached(float targetTime, Action action)
        {
            FermixCore.RunCoroutine(TimeReachedCoroutine(targetTime, action));
        }

        private static IEnumerator<float> TimeReachedCoroutine(float targetTime, Action action)
        {
            while (IsInProgress && TimeLeft > targetTime)
            {
                yield return Timing.WaitForSeconds(0.5f);
            }
            
            if (IsInProgress && TimeLeft <= targetTime)
            {
                action?.Invoke();
            }
        }

        #endregion

        #region Special Operations - Специальные Операции

        /// <summary>
        /// Запускает боеголовку, которую нельзя остановить.
        /// </summary>
        public static void StartUnstoppable(float? time = null)
        {
            if (time.HasValue)
            {
                SetTime(time.Value);
            }
            Start();
            Lock();
            FermixLog.Warn("Запущена неостановимая боеголовка!");
        }

        /// <summary>
        /// Запускает "фейковую" боеголовку (звук без взрыва).
        /// </summary>
        public static void StartFake(float duration = 30f)
        {
            Start();
            FermixScheduler.Delay(duration, () =>
            {
                if (IsInProgress)
                {
                    Stop();
                    FermixLog.Info("Фейковая боеголовка остановлена");
                }
            });
        }

        /// <summary>
        /// Паникующая боеголовка (случайный таймер).
        /// </summary>
        public static void StartPanic()
        {
            var randomTime = UnityEngine.Random.Range(30f, 120f);
            StartWithTime(randomTime);
            FermixLog.Warn($"Паникующая боеголовка: {randomTime:F0} сек!");
        }

        /// <summary>
        /// Запускает последовательность из нескольких попыток взрыва.
        /// </summary>
        public static void StartSequence(int attempts, float interval, float duration)
        {
            FermixCore.RunCoroutine(SequenceCoroutine(attempts, interval, duration));
        }

        private static IEnumerator<float> SequenceCoroutine(int attempts, float interval, float duration)
        {
            for (int i = 0; i < attempts; i++)
            {
                StartWithTime(duration);
                FermixLog.Warn($"Попытка взрыва {i + 1}/{attempts}");
                
                yield return Timing.WaitForSeconds(Math.Max(0f, duration - 5f));
                
                if (IsInProgress)
                {
                    Stop();
                }
                
                yield return Timing.WaitForSeconds(interval);
            }
        }

        #endregion

        #region Shake Effects - Эффекты Тряски

        /// <summary>
        /// Создаёт эффект приближающегося взрыва (тряска экрана).
        /// </summary>
        public static void ShakeScreen(byte intensity = 10)
        {
            foreach (var player in Player.List)
            {
                player.EnableEffect(Exiled.API.Enums.EffectType.Traumatized, intensity, 3f);
            }
        }

        /// <summary>
        /// Эффект приближающегося взрыва с нарастающей интенсивностью.
        /// </summary>
        public static void BuildingShake()
        {
            FermixCore.RunCoroutine(BuildingShakeCoroutine());
        }

        private static IEnumerator<float> BuildingShakeCoroutine()
        {
            while (IsInProgress && TimeLeft > 0)
            {
                byte intensity = (byte)Math.Max(0, Math.Min(255, (90 - TimeLeft) * 3));
                ShakeScreen(intensity);
                yield return Timing.WaitForSeconds(5f);
            }
        }

        #endregion
    }
}
