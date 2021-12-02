using Base;
using Enumerations;
using Peripherals;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinkerTank.Abstractions
{
    public class MovementAbstractions : TinkerBase, ITinkerBase
    {
        public IMovementInterface MovementController { get; set; }
        public decimal AutoStopDistance { get; set; }
        private int defaultPower = 75;
        private bool defaultSafeMove = true;
        private int defaultAccelleration = 10;
        private TimeSpan defaultMovementDuration = TimeSpan.FromSeconds(1);

        public MovementAbstractions(IMovementInterface movementController)
        {
            if (movementController == null)
            {
                throw new NullReferenceException();
            }

            MovementController = movementController;
        }

        public void Move(Direction directionToMove)
        {
            Move(directionToMove, defaultPower, defaultSafeMove, defaultAccelleration, defaultMovementDuration);
        }

        public void Move(Direction directionToMove, int power)
        {
            Move(directionToMove, power, defaultSafeMove, defaultAccelleration, defaultMovementDuration);
        }

        public void Move(Direction directionToMove, int power, bool safeMove)
        {
            Move(directionToMove, power, safeMove, defaultAccelleration, defaultMovementDuration);
        }

        public void Move(Direction directionToMove, int power, bool safeMove, int accelleration)
        {
            Move(directionToMove, power, safeMove, accelleration, defaultMovementDuration);
        }

        public void Move(Direction directionToMove, int power, bool safeMove, int accelleration, TimeSpan movementDuration )
        {
            MovementController.Move(directionToMove, power, safeMove, accelleration, movementDuratio)
            //Move(directionToMove, power, safeMove, accelleration, movementDuration);
        }

        public void ErrorEncountered()
        {
            throw new NotImplementedException();
        }

        public void RefreshStatus()
        {
            throw new NotImplementedException();
        }

        public void Test()
        {
            throw new NotImplementedException();
        }
    }
}
