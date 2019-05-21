﻿using ISAAR.MSolve.Analyzers;
using ISAAR.MSolve.Analyzers.Interfaces;
using ISAAR.MSolve.FEM.Elements;
using ISAAR.MSolve.FEM.Elements.SupportiveClasses;
using ISAAR.MSolve.FEM.Entities;
using ISAAR.MSolve.FEM.Materials;
using ISAAR.MSolve.Numerical.LinearAlgebra;
using ISAAR.MSolve.Problems;
using ISAAR.MSolve.Solvers.Interfaces;
using ISAAR.MSolve.Solvers.Skyline;
using System;
using System.Collections.Generic;
using System.Text;
using ISAAR.MSolve.Discretization.Interfaces;
using Xunit;
using ISAAR.MSolve.Materials;
using ISAAR.MSolve.FEM;
using ISAAR.MSolve.Discretization.Providers;
using ISAAR.MSolve.Logging;
using ISAAR.MSolve.FEM.Embedding;
using System.Linq;
using ISAAR.MSolve.Discretization.Integration.Quadratures;
using ISAAR.MSolve.Solvers.Direct;
using ISAAR.MSolve.Analyzers.NonLinear;
using ISAAR.MSolve.Discretization;

namespace ISAAR.MSolve.Tests
{
    public class EmbeddedElementTechniqueTests
    {
        private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\MSolveResults";
        private const int subdomainID = 1;

        [Fact]
        public void EmbeddedElementTechniqueExample()
        {
            VectorExtensions.AssignTotalAffinityCount();
            var model = new Model_v2();
            model.SubdomainsDictionary.Add(1, new Subdomain_v2(1));

            // Choose model
            EmbeddedExamplesBuilder.ExampleWithEmbedded(model);

            // Boundary Conditions - In Hexa8 - Host Element
            model.NodesDictionary[2].Constraints.Add(new Constraint { DOF = DOFType.X });
            model.NodesDictionary[2].Constraints.Add(new Constraint { DOF = DOFType.Y });
            model.NodesDictionary[2].Constraints.Add(new Constraint { DOF = DOFType.Z });
            model.NodesDictionary[3].Constraints.Add(new Constraint { DOF = DOFType.X });
            model.NodesDictionary[3].Constraints.Add(new Constraint { DOF = DOFType.Y });
            model.NodesDictionary[3].Constraints.Add(new Constraint { DOF = DOFType.Z });
            model.NodesDictionary[6].Constraints.Add(new Constraint { DOF = DOFType.X });
            model.NodesDictionary[6].Constraints.Add(new Constraint { DOF = DOFType.Y });
            model.NodesDictionary[6].Constraints.Add(new Constraint { DOF = DOFType.Z });
            model.NodesDictionary[7].Constraints.Add(new Constraint { DOF = DOFType.X });
            model.NodesDictionary[7].Constraints.Add(new Constraint { DOF = DOFType.Y });
            model.NodesDictionary[7].Constraints.Add(new Constraint { DOF = DOFType.Z });

            // Add nodal load values at the top nodes of the model
            double loading = 10.0;
            model.Loads.Add(new Load_v2() { Amount = loading, Node = model.NodesDictionary[1], DOF = DOFType.Z });
            model.Loads.Add(new Load_v2() { Amount = loading, Node = model.NodesDictionary[4], DOF = DOFType.Z });
            model.Loads.Add(new Load_v2() { Amount = loading, Node = model.NodesDictionary[5], DOF = DOFType.Z });
            model.Loads.Add(new Load_v2() { Amount = loading, Node = model.NodesDictionary[8], DOF = DOFType.Z });

            // Choose linear equation system solver
            SkylineSolver solver = (new SkylineSolver.Builder()).BuildSolver(model);

            // Choose the provider of the problem -> here a structural problem
            var provider = new ProblemStructural_v2(model, solver);

            // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer
            int increments = 10;
            var loadControlBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments);
            LoadControlAnalyzer_v2 childAnalyzer = loadControlBuilder.Build();

            // Choose parent analyzer -> Parent: Static
            var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

            // Request output
            childAnalyzer.LogFactories[1] = new LinearAnalyzerLogFactory_v2(new int[] { 11 });
            // print load-displacement curve
            string outputFile = outputDirectory + "\\EmbeddedElementTechniqueExample_Results.txt";
            int monitorNode = 8;
            DOFType monitorDof = DOFType.Z;
            var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                model.NodesDictionary[monitorNode], monitorDof, outputFile);
            childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

            // Run
            parentAnalyzer.Initialize();
            parentAnalyzer.Solve();

            // Check output
            DOFSLog_v2 log = (DOFSLog_v2)childAnalyzer.Logs[1][0]; //There is a list of logs for each subdomain and we want the first one (index = 0) from subdomain id = 1
            var computedValue = log.DOFValues[11];
            Assert.Equal(9.2128510065429143, computedValue, 3);
        }

        [Fact]
        public void EmbeddedElementTechniqueWithCohesiveBeamExample()
        {
            VectorExtensions.AssignTotalAffinityCount();
            var model = new Model_v2();
            model.SubdomainsDictionary.Add(1, new Subdomain_v2(1));

            // Choose model
            EmbeddedExamplesBuilder.ExampleWithEmbeddedCohesive(model);

            // Boundary Conditions - In Hexa8 - Host Element
            model.NodesDictionary[2].Constraints.Add(new Constraint { DOF = DOFType.X });
            model.NodesDictionary[2].Constraints.Add(new Constraint { DOF = DOFType.Y });
            model.NodesDictionary[2].Constraints.Add(new Constraint { DOF = DOFType.Z });
            model.NodesDictionary[3].Constraints.Add(new Constraint { DOF = DOFType.X });
            model.NodesDictionary[3].Constraints.Add(new Constraint { DOF = DOFType.Y });
            model.NodesDictionary[3].Constraints.Add(new Constraint { DOF = DOFType.Z });
            model.NodesDictionary[6].Constraints.Add(new Constraint { DOF = DOFType.X });
            model.NodesDictionary[6].Constraints.Add(new Constraint { DOF = DOFType.Y });
            model.NodesDictionary[6].Constraints.Add(new Constraint { DOF = DOFType.Z });
            model.NodesDictionary[7].Constraints.Add(new Constraint { DOF = DOFType.X });
            model.NodesDictionary[7].Constraints.Add(new Constraint { DOF = DOFType.Y });
            model.NodesDictionary[7].Constraints.Add(new Constraint { DOF = DOFType.Z });
            //model.NodesDictionary[1].Constraints.Add(new Constraint { DOF = DOFType.X });
            //model.NodesDictionary[1].Constraints.Add(new Constraint { DOF = DOFType.Y });
            //model.NodesDictionary[1].Constraints.Add(new Constraint { DOF = DOFType.Z });
            //model.NodesDictionary[4].Constraints.Add(new Constraint { DOF = DOFType.X });
            //model.NodesDictionary[4].Constraints.Add(new Constraint { DOF = DOFType.Y });
            //model.NodesDictionary[4].Constraints.Add(new Constraint { DOF = DOFType.Z });
            //model.NodesDictionary[5].Constraints.Add(new Constraint { DOF = DOFType.X });
            //model.NodesDictionary[5].Constraints.Add(new Constraint { DOF = DOFType.Y });
            //model.NodesDictionary[5].Constraints.Add(new Constraint { DOF = DOFType.Z });
            //model.NodesDictionary[8].Constraints.Add(new Constraint { DOF = DOFType.X });
            //model.NodesDictionary[8].Constraints.Add(new Constraint { DOF = DOFType.Y });
            //model.NodesDictionary[8].Constraints.Add(new Constraint { DOF = DOFType.Z });

            // Choose linear equation system solver
            SkylineSolver solver = (new SkylineSolver.Builder()).BuildSolver(model);

            // Choose the provider of the problem -> here a structural problem
            var provider = new ProblemStructural_v2(model, solver);

            // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer
            int increments = 10;
            var loadControlBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments);
            LoadControlAnalyzer_v2 childAnalyzer = loadControlBuilder.Build();

            // Choose parent analyzer -> Parent: Static
            var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

            // Loading Conditions - in Beam3DQuaternion Element - Embedded Element
            //double loading = 100.0;
            //model.Loads.Add(new Load_v2() { Amount = loading, Node = model.NodesDictionary[10], DOF = DOFType.X });
            double loading = 10.0;
            model.Loads.Add(new Load_v2() { Amount = loading, Node = model.NodesDictionary[1], DOF = DOFType.Z });
            model.Loads.Add(new Load_v2() { Amount = loading, Node = model.NodesDictionary[4], DOF = DOFType.Z });
            model.Loads.Add(new Load_v2() { Amount = loading, Node = model.NodesDictionary[5], DOF = DOFType.Z });
            model.Loads.Add(new Load_v2() { Amount = loading, Node = model.NodesDictionary[8], DOF = DOFType.Z });

            // Request output
            childAnalyzer.LogFactories[1] = new LinearAnalyzerLogFactory_v2(new int[] { 11 }); // or 6
            // print load-displacement curve
            string outputFile = outputDirectory + "\\EmbeddedElementTechniqueWithCohesiveBeamExample_Results.txt";
            int monitorNode = 8;
            DOFType monitorDof = DOFType.Z;
            var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                model.NodesDictionary[monitorNode], monitorDof, outputFile);
            childAnalyzer.IncrementalLogs.Add(subdomainID, logger);
            
            // Run
            parentAnalyzer.Initialize();
            parentAnalyzer.Solve();

            // Check output
            DOFSLog_v2 log = (DOFSLog_v2)childAnalyzer.Logs[1][0]; //There is a list of logs for each subdomain and we want the first one (index = 0) from subdomain id = 1
            var computedValue = log.DOFValues[11];
            Assert.Equal(13.534664719799263, computedValue, 3);
        }

        public static class EmbeddedExamplesBuilder
        {
            public static void HostElementsBuilder(Model_v2 model)
            {
                // Nodes Geometry
                model.NodesDictionary.Add(1, new Node_v2() { ID = 1, X = 10.00, Y = 2.50, Z = 2.50 });
                model.NodesDictionary.Add(2, new Node_v2() { ID = 2, X = 0.00, Y = 2.50, Z = 2.50 });
                model.NodesDictionary.Add(3, new Node_v2() { ID = 3, X = 0.00, Y = -2.50, Z = 2.50 });
                model.NodesDictionary.Add(4, new Node_v2() { ID = 4, X = 10.00, Y = -2.50, Z = 2.50 });
                model.NodesDictionary.Add(5, new Node_v2() { ID = 5, X = 10.00, Y = 2.50, Z = -2.50 });
                model.NodesDictionary.Add(6, new Node_v2() { ID = 6, X = 0.00, Y = 2.50, Z = -2.50 });
                model.NodesDictionary.Add(7, new Node_v2() { ID = 7, X = 0.00, Y = -2.50, Z = -2.50 });
                model.NodesDictionary.Add(8, new Node_v2() { ID = 8, X = 10.00, Y = -2.50, Z = -2.50 });

                // Create Material
                var solidMaterial = new ElasticMaterial3D_v2()
                {
                    YoungModulus = 3.76,
                    PoissonRatio = 0.3779,
                };

                // Hexa8NL element definition
                var hexa8NLelement = new Element_v2()
                {
                    ID = 1,
                    ElementType = new Hexa8NonLinear_v2(solidMaterial, GaussLegendre3D.GetQuadrature(3, 3, 3))
                };

                // Add nodes to the created element
                hexa8NLelement.AddNode(model.NodesDictionary[1]);
                hexa8NLelement.AddNode(model.NodesDictionary[2]);
                hexa8NLelement.AddNode(model.NodesDictionary[3]);
                hexa8NLelement.AddNode(model.NodesDictionary[4]);
                hexa8NLelement.AddNode(model.NodesDictionary[5]);
                hexa8NLelement.AddNode(model.NodesDictionary[6]);
                hexa8NLelement.AddNode(model.NodesDictionary[7]);
                hexa8NLelement.AddNode(model.NodesDictionary[8]);

                // Add Hexa element to the element and subdomains dictionary of the model
                model.ElementsDictionary.Add(hexa8NLelement.ID, hexa8NLelement);
                model.SubdomainsDictionary[1].Elements.Add(hexa8NLelement);

                // Add nodal load values at the top nodes of the model
                //double loading = 10.0;
                //model.Loads.Add(new Load_v2() { Amount = loading, Node = model.NodesDictionary[1], DOF = DOFType.Z });
                //model.Loads.Add(new Load_v2() { Amount = loading, Node = model.NodesDictionary[4], DOF = DOFType.Z });
                //model.Loads.Add(new Load_v2() { Amount = loading, Node = model.NodesDictionary[5], DOF = DOFType.Z });
                //model.Loads.Add(new Load_v2() { Amount = loading, Node = model.NodesDictionary[8], DOF = DOFType.Z });
            }

            public static void EmbeddedElementsBuilder(Model_v2 model)
            {
                // define mechanical properties
                double youngModulus = 1.0;
                double shearModulus = 1.0;
                double poissonRatio = (youngModulus / (2 * shearModulus)) - 1;
                double area = 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                double inertiaY = 1058.55;
                double inertiaZ = 1058.55;
                double torsionalInertia = 496.38;
                double effectiveAreaY = area;
                double effectiveAreaZ = area;

                // Geometry
                model.NodesDictionary.Add(9, new Node_v2() { ID = 9, X = 0.00, Y = 0.00, Z = 0.00 });
                model.NodesDictionary.Add(10, new Node_v2() { ID = 10, X = 10.00, Y = 0.00, Z = 0.00 });

                // Create new 3D material
                var beamMaterial = new ElasticMaterial3D_v2
                {
                    YoungModulus = youngModulus,
                    PoissonRatio = poissonRatio,
                };

                // Create new Beam3D section and element
                var beamSection = new BeamSection3D(area, inertiaY, inertiaZ, torsionalInertia, effectiveAreaY, effectiveAreaZ);
                // element nodes
                var elementNodes = new List<Node_v2>();
                elementNodes.Add(model.NodesDictionary[9]);
                elementNodes.Add(model.NodesDictionary[10]);
                var beam = new Beam3DCorotationalQuaternion_v2(elementNodes, beamMaterial, 7.85, beamSection);
                var beamElement = new Element_v2 { ID = 2, ElementType = beam };

                beamElement.NodesDictionary.Add(9, model.NodesDictionary[9]);
                beamElement.NodesDictionary.Add(10, model.NodesDictionary[10]);

                model.ElementsDictionary.Add(beamElement.ID, beamElement);
                model.SubdomainsDictionary[1].Elements.Add(beamElement);                
            }

            public static void CohesiveBeamElementBuilder(Model_v2 model)
            {
                // define mechanical properties
                double youngModulus = 1.0;
                double shearModulus = 1.0;
                double poissonRatio = (youngModulus / (2 * shearModulus)) - 1;
                double area = 1776.65;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                double inertiaY = 1058.55;
                double inertiaZ = 1058.55;
                double torsionalInertia = 496.38;
                double effectiveAreaY = area;
                double effectiveAreaZ = area;

                // Geometry
                model.NodesDictionary.Add(11, new Node_v2() { ID = 11, X = 0.00, Y = 0.00, Z = 0.00 });
                model.NodesDictionary.Add(12, new Node_v2() { ID = 12, X = 10.00, Y = 0.00, Z = 0.00 });

                // element nodes
                var elementNodesClone = new List<Node_v2>();
                elementNodesClone.Add(model.NodesDictionary[11]);
                elementNodesClone.Add(model.NodesDictionary[12]);
                var elementNodesBeam = new List<Node_v2>();
                elementNodesBeam.Add(model.NodesDictionary[9]);
                elementNodesBeam.Add(model.NodesDictionary[10]);

                // Create Cohesive Material
                var cohesiveMaterial = new BondSlipCohMat_v2(100, 10, 100, 10, 1, new double[2], new double[2], 1e-10);                
                //var cohesiveMaterial = new BondSlipCohMat_v2(5.0, 5.0, 5.0, 100.0, new double[2], new double[2], 1e-10);
                
                // Create Elastic 3D Material
                var elasticMaterial = new ElasticMaterial3D_v2
                {
                    YoungModulus = youngModulus,
                    PoissonRatio = poissonRatio,
                };

                // Create Beam3D Section
                var beamSection = new BeamSection3D(area, inertiaY, inertiaZ, torsionalInertia, effectiveAreaY, effectiveAreaZ);

                // Create Cohesive Beam Element
                var cohesiveElement = new Element_v2()
                {
                    ID = 3,
                    ElementType = new CohesiveBeam3DToBeam3D(cohesiveMaterial, GaussLegendre1D.GetQuadrature(2), elementNodesBeam,
                        elementNodesClone, elasticMaterial, 1, beamSection)
                };

                // Add Cohesive Beam Element in Model
                model.ElementsDictionary.Add(3, cohesiveElement);

                // Add Cohesive Element Nodes (!)
                model.ElementsDictionary[3].AddNode(model.NodesDictionary[9]);
                model.ElementsDictionary[3].AddNode(model.NodesDictionary[10]);
                model.ElementsDictionary[3].AddNode(model.NodesDictionary[11]);
                model.ElementsDictionary[3].AddNode(model.NodesDictionary[12]);

                // Add Cohesive Element in Subdomain
                model.SubdomainsDictionary[1].Elements.Add(cohesiveElement);
            }

            public static void ExampleWithEmbedded(Model_v2 model)
            {
                HostElementsBuilder(model);
                EmbeddedElementsBuilder(model);
                var embeddedGrouping = new EmbeddedGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key == 1).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key == 2).Select(kv => kv.Value), true);
            }

            public static void ExampleWithEmbeddedCohesive(Model_v2 model)
            {
                HostElementsBuilder(model);
                EmbeddedElementsBuilder(model);
                CohesiveBeamElementBuilder(model);
                var embeddedGrouping = new EmbeddedCohesiveBeam3DGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key == 1).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key == 3).Select(kv => kv.Value), true);
            }
        }
    }
}
