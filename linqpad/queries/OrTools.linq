<Query Kind="Program">
  <Output>DataGrids</Output>
  <GACReference>System.IO.Compression, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</GACReference>
  <GACReference>System.IO.Compression.FileSystem, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</GACReference>
  <NuGetReference>Google.OrTools.x86</NuGetReference>
  <Namespace>Google.OrTools.ConstraintSolver</Namespace>
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Diagnostics</Namespace>
  <Namespace>System.Diagnostics.Contracts</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.IO.Compression</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Linq.Expressions</Namespace>
  <Namespace>System.Reflection</Namespace>
  <Namespace>System.Threading</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

static void Main()
{
	// global constraint catalog: http://sofdem.github.io/gccat/
	Grocery();
	Cryptogram();
}

static void Cryptogram()
{
	using (var solver = new Solver("Cryptogram"))
	{
		// One decision variable for each character:
		using (var S = solver.MakeIntVar(0, 9))
		using (var E = solver.MakeIntVar(0, 9))
		using (var N = solver.MakeIntVar(0, 9))
		using (var D = solver.MakeIntVar(0, 9))
		using (var M = solver.MakeIntVar(0, 9))
		using (var O = solver.MakeIntVar(0, 9))
		using (var R = solver.MakeIntVar(0, 9))
		using (var Y = solver.MakeIntVar(0, 9))
		{
			using (var send = (S * 1000 + E * 100 + N * 10 + D).Var())
			using (var more = (M * 1000 + O * 100 + R * 10 + E).Var())
			using (var money = (M * 10000 + O * 1000 + N * 100 + E * 10 + Y).Var())
			{
				solver.Add(send + more == money); // main constraint
												  // Leading characters must not be zero:
				solver.Add(S != 0);
				solver.Add(M != 0);
				var allVariables = new IntVar[] { S, E, N, D, M, O, R, Y };
				// THIS DOES NOT WORK! using(solver.MakeAllDifferent(allVariables))
				solver.Add(allVariables.AllDifferent());
				using (DecisionBuilder db = solver.MakePhase(
					allVariables, // Decision variables to resolve
					Solver.INT_VAR_SIMPLE, // Variable selection policy for search
					Solver.INT_VALUE_SIMPLE)) // Value selection policy for search
				{
					solver.NewSearch(db);
				}
				while (solver.NextSolution())
				{
					Console.WriteLine(send.Value() + "+" + more.Value() + "=" + money.Value());
				}
				solver.EndSearch();
				// Display the number of solutions found:
				Console.WriteLine("Solutions: " + solver.Solutions());
			}
		}
	}
}

static void Grocery()
{
	using (var solver = new Solver("Grocery")) // create solver
	{
		// One variable for each product. Range of values from 0 to 711
		using (var p1 = solver.MakeIntVar(0, 711))
		using (var p2 = solver.MakeIntVar(0, 711))
		using (var p3 = solver.MakeIntVar(0, 711))
		using (var p4 = solver.MakeIntVar(0, 711))
		{
			// Prices add up to 711:
			solver.Add(p1 + p2 + p3 + p4 == 711);
			// Product of prices is 711. (since we use cent values, we don't have 7.11 but 7.11*10^8 because we have 4 factors)
			solver.Add(p1 * p2 * p3 * p4 == 711 * 100 * 100 * 100);

			// break symmetry.
			solver.Add(p1 <= p2);
			solver.Add(p2 <= p3);
			solver.Add(p3 <= p4);

			using (var decisionBuilder = solver.MakePhase(
				new IntVar[] { p1, p2, p3, p4 }, // Decision variables to resolve
				Solver.INT_VAR_SIMPLE, // Variable selection policy for search
				Solver.INT_VALUE_SIMPLE))
			{ // Value selection policy for search
				solver.NewSearch(decisionBuilder);
			}

			while (solver.NextSolution())
			{
				Console.WriteLine("Product 1: " + p1.Value());
				Console.WriteLine("Product 2: " + p2.Value());
				Console.WriteLine("Product 3: " + p3.Value());
				Console.WriteLine("Product 4: " + p4.Value());
				Console.WriteLine();
			}
			solver.EndSearch();
		}
	}
}