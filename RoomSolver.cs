using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace med_room
{
    public class RoomSolver
    {
        public RoomSolver()
        {
        }

        public decimal Result { get; set; }

        public IList<Operation> SolveTimeSlot(IList<Operation> operations, int time, int nbOperationLimit)
        {
            var allLimit = operations.Select(x => x.LimitVar).Distinct();
            var allMemo = new List<decimal[,]>();
            var bestSoFar = 0.0m;
            decimal[,] bestMemo = null;
            var limitForBestMemo = string.Empty;
            foreach (var limit in allLimit)
            {
                var operationForLimit = operations.Where(x => x.LimitVar == limit).ToList();
                var memo = this.PackIteratif(operationForLimit, operationForLimit.Count - 1, time, nbOperationLimit);
                allMemo.Add(memo);
                var result = memo[operationForLimit.Count - 1, time];
                if (result > bestSoFar)
                {
                    bestSoFar = result;
                    bestMemo = memo;
                    limitForBestMemo = limit;
                }
            }

            var list = new List<Operation>();
            var operationForSameLimit = operations.Where(x => x.LimitVar == limitForBestMemo).ToList();
            this.RetriedFittedOperations(bestMemo, list, operationForSameLimit, operationForSameLimit.Count, time);

            return list;
        }

        public IList<OperationResult> Solve(Week week, IList<Operation> operations)
        {
            //var ordered = this._operations
            //    .OrderByDescending()
            // this.Result = this.Pack(this._operations, this._operations.Count - 1, this._timeSlot.RemainingTime, this._timeSlot.Type);
            var selectedOperations = new List<Operation>();
            foreach (var weekTimeSlot in week.TimeSlots)
            {
                var operationAvailable = operations.Where(x => selectedOperations.All(y => y.Id != x.Id)).ToList();
                var timeSlotOperationBatch = this.SolveTimeSlot(operationAvailable, weekTimeSlot.RemainingTime, week.NbOperationLimit);
                selectedOperations.AddRange(timeSlotOperationBatch);
            }

            return selectedOperations.Select(x => new OperationResult
            {
                Duree = x.Duree,
                Id = x.Id,
                LimitVar = x.LimitVar,
                ScoreOp = x.ScoreOp,
                SemaineDispo = x.SemaineDispo,
                WeekFitted = week.Number
            }).ToList();
        }

        public decimal[,] PackIteratif(IList<Operation> operations, int index, int timeRemaining, int nbOperationLimit)
        {
            var memo = new decimal[operations.Count + 1, timeRemaining + 1];

            for (var i = 1; i <= operations.Count; i++)
            {
                for (var j = 0; j <= timeRemaining; j++)
                {
                    if (operations[i - 1].Duree <= j)
                    {
                        var lastScore = memo[i - 1, j];// for this opperation and this time
                        var lastScoreWithTimeRemainingBeforeThisOperation = memo[i - 1, j - operations[i - 1].Duree];
                        var nextScore = lastScoreWithTimeRemainingBeforeThisOperation + operations[i - 1].ScoreOp;
                        memo[i, j] = Math.Max(lastScore, nextScore);
                    }
                    else
                    {
                        // le score est le meme que pour le dernier puis qu'on ne peut pas inserer une autre operation
                        memo[i, j] = memo[i - 1, j];
                    }
                }
            }

            return memo;
        }

        private void RetriedFittedOperations(decimal[,] memo, IList<Operation> retrievedOperations, IList<Operation> allOperations, int index, int time)
        {
            if (index == 0)
            {
                return;
            }

            if (memo[index, time] > memo[index - 1, time])
            {
                retrievedOperations.Add(allOperations[index - 1]);
                this.RetriedFittedOperations(memo, retrievedOperations, allOperations, index - 1, time - allOperations[index - 1].Duree);
            }
            else
            {
                this.RetriedFittedOperations(memo, retrievedOperations, allOperations, index - 1, time);
            }
        }

        private decimal Pack(decimal[,] memo, IList<Operation> operations, int index, int timeRemaining, string type)
        {
            if (index < 0)
            {
                return 0;
            }

            if (timeRemaining <= 0)
            {
                return 0;
            }

            if (memo[index, timeRemaining] != -1)
            {
                return memo[index, timeRemaining];
            }

            var result = 0.0m;
            for (var time = 0; time < timeRemaining; time++)
            {
                var resultForThisTime = 0.0m;

                // si je peux placer cette operation a ce temps
                if (time >= operations[index].Duree)
                {
                    var scoreOp = operations[index].ScoreOp;
                    var timeAfterOperation = time - operations[index].Duree;

                    var resultIfWeSkipIt = this.Pack(memo, operations, index - 1, time, type);
                    var resultIfWeTakeIt = this.Pack(memo, operations, index - 1, timeAfterOperation, type) + scoreOp;

                    resultForThisTime = Math.Max(resultIfWeSkipIt, resultIfWeTakeIt);
                }
                else
                {
                    // on peut pas le placer donc on le skip
                    resultForThisTime = this.Pack(memo, operations, index - 1, time, type);
                }

                if (resultForThisTime > result)
                {
                    result = resultForThisTime;
                }
            }

            memo[index, timeRemaining] = result;

            return result;
        }
    }

    // Mon vieu code de knapsack que j'ai fais ya longtemps
    /*
      public static int pack(int index,List<int[]> items, int w){

        if(index == nb_items){
            return 0;
        }
        if(w ==0)return 0;
        if(index < 0){
            return 0;
        }



        if(memo[index][w] != -1){
            return memo[index][w];
        }

        int res =0;
        for(int i=0; i <= w ; i++){
            if( items.get(index)[0] <= i){
                int w2 = i - items.get(index)[0] ;


                res=  Math.max(pack(index-1,items,i), pack(index-1,items,w2) + items.get(index)[1] );

            }else{
                res =  pack(index -1,items, i);
            }

        }

        memo[index][w] = res;
        return res;
    }
     */
}