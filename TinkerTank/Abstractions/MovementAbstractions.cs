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
        private bool defaultSmoothAccelleration = true;
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
            Move(directionToMove, defaultPower, defaultSafeMove, defaultSmoothAccelleration, defaultMovementDuration);
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
            //MovementController.Move(directionToMove, power, safeMove, smoothAccelleration);//, movementDuration);
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
