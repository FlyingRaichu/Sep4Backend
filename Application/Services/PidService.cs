using Application.ServiceInterfaces;

namespace Application.Services
{
    public class PidService : IPidService
    {
        private double ProportionalGain { get; set; }
        private double IntegralGain { get; set; }
        private double DerivativeGain { get; set; }
        private double DesiredFlowRate { get; set; }

        private double _integralSum;
        private double _previousError;

        public PidService(double proportionalGain, double integralGain, double derivativeGain, double desiredFlowRate)
        {
            ProportionalGain = proportionalGain;
            IntegralGain = integralGain;
            DerivativeGain = derivativeGain;
            DesiredFlowRate = desiredFlowRate;
            _integralSum = 0;
            _previousError = 0;
        }
        
        public int Compute(float currentFlowRate, double deltaTimeInSeconds)
        {
            if (deltaTimeInSeconds <= 0)
            {
                throw new ArgumentException("deltaTime must be greater than zero", nameof(deltaTimeInSeconds));
            }

            // Error calculation
            var error = DesiredFlowRate - currentFlowRate;

            // Proportional term
            var proportionalTerm = ProportionalGain * error;

            // Integral term
            _integralSum += error * deltaTimeInSeconds;
            var integralTerm = IntegralGain * _integralSum;

            // Derivative term
            var derivative = (error - _previousError) / deltaTimeInSeconds;
            var derivativeTerm = DerivativeGain * derivative;
            
            _previousError = error;
            
            var output = proportionalTerm + integralTerm + derivativeTerm;

            // Clamp output to 0% - 100%
            output = Math.Max(0, Math.Min(100, output));

            // Clamp integral value if it overflows below 0% or over 100%
            if (output is 0 or 100)
            {
                _integralSum -= error * deltaTimeInSeconds;  // Undo integral term update if output is clamped
            }
            
            return (int)Math.Floor(output);
        }
    }
}
