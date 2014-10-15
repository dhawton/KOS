﻿using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace kOS.Suffixed.Part
{
    public class PartValue : Structure, IKOSTargetable
    {
        private readonly SharedObjects shared;

        protected global::Part Part { get; private set; }

        public PartValue(global::Part part, SharedObjects sharedObj)
        {
            Part = part;
            shared = sharedObj;
            PartInitializeSuffixes();
        }

        private void PartInitializeSuffixes()
        {
            AddSuffix("CONTROLFROM", new NoArgsSuffix(ControlFrom));
            AddSuffix("NAME", new Suffix<string>(() => Part.name));
            AddSuffix("STAGE", new Suffix<int>(() => Part.inverseStage));
            AddSuffix("UID", new Suffix<uint>(() => Part.uid));
            AddSuffix("ROTATION", new Suffix<Direction>(() => new Direction(Part.orgRot)));
            AddSuffix("POSITION", new Suffix<Vector>(() => new Vector(Part.orgPos)));
            AddSuffix("FACING", new Suffix<Direction>(() => GetFacing(Part)));
            AddSuffix("RESOURCES", new Suffix<ListValue>(() => GatherResources(Part)));
            AddSuffix("MODULES", new Suffix<ListValue>(() => GatherModules(Part)));
            AddSuffix("TARGETABLE", new Suffix<bool>(() => Part.Modules.OfType<ITargetable>().Any()));
            AddSuffix("SHIP", new Suffix<VesselTarget>(() => new VesselTarget(Part.vessel, shared)));
        }

        public override string ToString()
        {
            return string.Format("PART({0},{1})", Part.name, Part.uid);
        }

        public static ListValue PartsToList(IEnumerable<global::Part> parts, SharedObjects sharedObj)
        {
            var toReturn = new ListValue();
            foreach (var part in parts)
            {
                toReturn.Add(new PartValue(part, sharedObj));
            }
            return toReturn;
        }

        public virtual ITargetable Target
        {
            get
            {
                return Part.Modules.OfType<ITargetable>().FirstOrDefault();
            }
        }

        private Direction GetFacing(global::Part part)
        {
            Vector3d up = part.vessel.upAxis;
            var partVec = part.partTransform.forward;

            var d = new Direction { Rotation = Quaternion.LookRotation(partVec, up) };
            return d;
        }

        private void ControlFrom()
        {
            var dockingModule = Part.Modules.OfType<ModuleDockingNode>().First();
            var commandModule = Part.Modules.OfType<ModuleCommand>().First();

            if (commandModule != null)
            {
                commandModule.MakeReference();
            }
            else if (dockingModule != null)
            {
                dockingModule.MakeReferenceTransform();
            }
            else
            {
                Part.vessel.SetReferenceTransform(Part);
            }
        }

        private ListValue GatherResources(global::Part part)
        {
            var resources = new ListValue();
            foreach (PartResource resource in part.Resources)
            {
                resources.Add(new ResourceValue(resource));
            }
            return resources;
        }

        private ListValue GatherModules(global::Part part)
        {
            var modules = new ListValue();
            foreach (var module in part.Modules)
            {
                modules.Add(module.GetType());
            }
            return modules;
        }
    }
}