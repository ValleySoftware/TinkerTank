using Base;
using Enumerations;
using Peripherals;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinkerTank.Abstractions
{
    public class MovementAbstractions : TrackControl
    {
        public IMovementInterface MovementController { get; set; }
        public decimal AutoStopDistance { get; set; }
        private bool defaultSafeMove = true;
        private bool defaultSmoothAccelleration = true;
        private TimeSpan defaultMovementDuration = TimeSpan.FromSeconds(1);

        public MovementAbstractions() : base()
        {
            SetDefaultPower(75);
        }

        public void Move(Direction directionToMove)
        {
            Move(directionToMove, _defaultPower, defaultSafeMove, defaultSmoothAccelleration, defaultMovementDuration);
        }

        public void Move(Direction directionToMove, int power)
        {
            Move(directionToMove, power, defaultSafeMove, defaultSmoothAccelleration, defaultMovementDuration);
        }

        public void Move(Direction directionToMove, int power, bool safeMove)
        {
            Move(directionToMove, power, safeMove, defaultSmoothAccelleration, defaultMovementDuration);
        }

        public void Move(Direction directionToMove, int power, bool safeMove, bool smoothAccelleration)
        {
            Move(directionToMove, power, safeMove, smoothAccelleration, defaultMovementDuration);
        }

        public void Move(Direction directionToMove, int power, bool safeMove, bool smoothAccelleration, TimeSpan movementDuration )
        {
            MovementController.Move(directionToMove, power, movementDuration, safeMove, smoothAccelleration);//, movementDuration);
        }
    }
}
