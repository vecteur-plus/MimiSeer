using SupervisorProcessing.Dao;
using SupervisorProcessing.Repository;
using System.Collections.Generic;
using System.Linq;

namespace SupervisorProcessing.Service
{
    public class ServiceFlag
    {
        private readonly FlagRepository _RepositoryFlag;

        public ServiceFlag(FlagRepository repoFlag_)
        {
            _RepositoryFlag = repoFlag_;
        }

        //set all flag to seen
        public void UpdatedFlagToSeen()
        {
            _RepositoryFlag.UpdateAllFlagToSeen();
        }

        //get deduplicated flag 
        public List<Flag> FindFlag()
        {
            List<Flag> FlagsDedoublenne;
            List<Flag> duplicatedFlags;

            var flags = _RepositoryFlag.FindFlags().ToList();

            FlagsDedoublenne = Deduplicate(flags);

            //get duplicated flag
            duplicatedFlags = flags.Except(FlagsDedoublenne).ToList();

            //set to seen duplicated flag
            if (UpdatedListflagToSeen(duplicatedFlags))
            {
                return FlagsDedoublenne;
            }

            return new();
        }

        //set to seen flag's list
        public bool UpdatedListflagToSeen(List<Flag> Flags_)
        {
            return _RepositoryFlag.UpdateListFlagToSeen(Flags_.Select(f => f.Id).ToList());
        }


        private List<Flag> Deduplicate(List<Flag> FlagsToDeduplicate_)
        {
            if (FlagsToDeduplicate_.Count == 0)
            {
                return new();
            }

            var flags = new List<Flag>();
            var LookFlag = FlagsToDeduplicate_.ToLookup(f => f.Name);

            foreach (var item in LookFlag)
            {
                
                if (item.Count() == 1)
                {
                    flags.Add(item.First());
                    continue;
                }
               
                if (item.Count(i => i.TypeModification == "ADD") > item.Count(i => i.TypeModification == "DELETE"))
                {
                    //check if pos ADD > pos DELETE
                    if (item.ToList().FindLastIndex(i => i.TypeModification == "ADD") > item.ToList().FindLastIndex(i => i.TypeModification == "DELETE"))
                    {
                        flags.Add(item.ToList().Find(i => i.TypeModification == "ADD"));
                        continue;
                    }
                }
                else if (item.Count(i => i.TypeModification == "ADD") < item.Count(i => i.TypeModification == "DELETE"))
                {
                    //check if pos ADD < pos DELETE
                    if (item.ToList().FindLastIndex(i => i.TypeModification == "ADD") < item.ToList().FindLastIndex(i => i.TypeModification == "DELETE"))
                    {
                        flags.Add(item.ToList().Find(i => i.TypeModification == "DELETE"));
                        continue;
                    }
                }
                else if (item.Count(i => i.TypeModification == "ADD") == item.Count(i => i.TypeModification == "DELETE"))
                {
                    // check if pos ADD > pos DELETE
                    if (item.ToList().FindLastIndex(i => i.TypeModification == "ADD") > item.ToList().FindLastIndex(i => i.TypeModification == "DELETE"))
                    {
                        flags.Add(new Flag() { Name = item.First().Name, TypeModification = "MODIFY" });
                        continue;
                    }
                    // check if pos ADD < pos DELETE
                    if (item.ToList().FindLastIndex(i => i.TypeModification == "ADD") < item.ToList().FindLastIndex(i => i.TypeModification == "DELETE"))
                    {
                        continue;
                    }
                }
            }

            return flags;
        }
    }
}