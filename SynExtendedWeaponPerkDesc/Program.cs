using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;

namespace SynExtendedWeaponPerkDesc
{
    public class Program
    {   


        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "SynExtendedWeaponPerkDesc.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var itemLink = new FormLink<IItemGetter>(FormKey.Factory("01E711:Skyrim.esm"));

            if (itemLink.TryResolve<IKeywordGetter>(state.LinkCache, out var itemRecord))
            {
                //Console.WriteLine($"Was able to find the item record object: {itemRecord}");
            }            
            IDictionary<string,bool> weapEnabled = new Dictionary<string,bool>()
            {
                {"Claw",false},
                {"Scythe",false},
                {"Whip",false},
                {"Javelin",false},
                {"Maul",false},
                {"Club",false},
                {"Hatchet",false},
                {"Glaive",false},
                {"Trident",false},
                {"Rapier",false},
                {"Pike",false},
                {"HalfPike",false},
                {"Spear",false},
                {"Poleaxe",false},
                {"Halberd",false},
                {"Quarterstaff",false},
            };
            foreach (IWeaponGetter weap in state.LoadOrder.PriorityOrder.OnlyEnabled().Weapon().WinningOverrides())
            {

                if(itemRecord==null){continue;}
                if(weap.Name == null){continue;}
                string weapName = weap.Name.String ?? "";
                if(weapName.Contains("Staff")||weap.MajorFlags.HasFlag(Weapon.MajorFlag.NonPlayable)){continue;}
                if(weapName.Contains("Claw")&&weap.HasKeyword(itemRecord)||(weap.EditorID ?? "").ToLower().Contains("dummy")){continue;}//Console.WriteLine($"{weapName}:{weap.EditorID}:Excluded");continue;}
                foreach(string weapType in weapEnabled.Keys)
                {
                    if(!weapEnabled[weapType])
                    {
                        if(weapName.Contains(weapType))
                        {
                            weapEnabled[weapType]=true;
                            Console.WriteLine($"Weapon Type: {weapType} found");
                            // if(weap.Keywords!=null){
                            //     foreach(var keyw in weap.Keywords)
                            //     {
                            //         string sKeyw = keyw.Resolve(state.LinkCache).EditorID ?? "";
                            //         if(sKeyw.ToLower().Contains("weaptype")){Console.WriteLine("Keyword: "+sKeyw);}
                            //     }
                            // }
                        }
                    }
                }
            }
            Dictionary<string,List<string>> baseWeapTypes = new Dictionary<string, List<string>>();// = {"dagger","mace","sword","waraxe","battleaxe","greatsword","warhammer"};
            baseWeapTypes["dagger"] = new List<string>();
            baseWeapTypes["dagger"].Add("Daggers");
            baseWeapTypes["mace"] = new List<string>();
            baseWeapTypes["mace"].Add("Maces");
            baseWeapTypes["sword"] = new List<string>();
            baseWeapTypes["sword"].Add("Swords");
            baseWeapTypes["waraxe"] = new List<string>();
            baseWeapTypes["waraxe"].Add("War Axes");
            baseWeapTypes["battleaxe"] = new List<string>();
            baseWeapTypes["battleaxe"].Add("Battle Axes");
            baseWeapTypes["greatsword"] = new List<string>();
            baseWeapTypes["greatsword"].Add("Greatswords");
            baseWeapTypes["warhammer"] = new List<string>();
            baseWeapTypes["warhammer"].Add("Warhammers");
            if(weapEnabled["Claw"]){baseWeapTypes["dagger"].Add("Claws");}
            if(weapEnabled["Scythe"]){baseWeapTypes["battleaxe"].Add("Scythes");}
            if(weapEnabled["Halberd"]){baseWeapTypes["battleaxe"].Add("Halberds");}
            if(weapEnabled["Whip"]){baseWeapTypes["sword"].Add("Whips");}
            if(weapEnabled["Javelin"]){baseWeapTypes["sword"].Add("Javelins");}
            if(weapEnabled["Maul"]){baseWeapTypes["warhammer"].Add("Mauls");}
            if(weapEnabled["Club"]){baseWeapTypes["warhammer"].Add("Clubs");}
            if(weapEnabled["Quarterstaff"]){baseWeapTypes["warhammer"].Add("Quarterstaves");}
            if(weapEnabled["Trident"]){baseWeapTypes["greatsword"].Add("Tridents");}
            if(weapEnabled["Glaive"]){baseWeapTypes["greatsword"].Add("Glaives");}
            if(weapEnabled["Spear"]){baseWeapTypes["greatsword"].Add("Spears");}
            if(weapEnabled["Spear"]){baseWeapTypes["sword"].Add("Shortspears");}
            if(weapEnabled["Pike"]){baseWeapTypes["greatsword"].Add("Pikes");}
            if(weapEnabled["Hatchet"]){baseWeapTypes["waraxe"].Add("Hatchets");}
            if(weapEnabled["Rapier"]){baseWeapTypes["sword"].Add("Rapiers");}

            foreach (IPerkGetter perk in state.LoadOrder.PriorityOrder.OnlyEnabled().Perk().WinningOverrides())
            {
                string perkDescStr = perk.Description.String ?? "blank";
                if(perkDescStr.Trim()==""||perkDescStr=="blank"){continue;}
                perkDescStr = perkDescStr.Replace("Great Sword","greatsword",true,null);
                perkDescStr = perkDescStr.Replace("War Hammer","warhammer",true,null);
                perkDescStr = perkDescStr.Replace("Battle axe","battleaxe",true,null);
                perkDescStr = perkDescStr.Replace("War axe","waraxe",true,null);

                
                bool modified = false;
                string line = wPerkReplacer(perkDescStr,baseWeapTypes,out modified);
                if(modified)
                {
                    var perkOverride = state.PatchMod.Perks.GetOrAddAsOverride(perk);
                    perkOverride.Description = line;
                    Console.WriteLine($"{perk.FormKey}:\nChanged Perk: {line}");
                }
                
            }
        }
        public static string wPerkReplacer(string perkDesc,Dictionary<string,List<string>> baseWeapTypes,out bool modified)
        {
            if(baseWeapTypes == null)
            {
                throw new System.ArgumentNullException();
            }
            string[] cutArray = perkDesc.Split(' ');
            perkDesc = perkDesc.ToLower();
            string[] cutNew = perkDesc.Split(' ');
            modified = false;
            foreach (string cWType in baseWeapTypes.Keys)
            {
                if(((cWType!="sword"&&perkDesc.Contains(cWType))||(cWType=="sword"&&perkDesc.Contains(cWType)&&!perkDesc.Contains("greatsword")))&&baseWeapTypes[cWType].Count>1)
                {

                    for(int i = 0;i<cutArray.Count();i++)
                    {
                        if(((cWType!="sword"&&cutNew[i].Contains(cWType))||(cWType=="sword"&&cutNew[i].Contains(cWType)&&!cutNew[i].Contains("greatsword"))))
                        {
                            if(i>0)
                            {
                                if(cutNew[i-1]=="a"){cutArray[i-1]="";}
                            }
                            cutArray[i]=baseWeapTypes[cWType][0];
                            for(int j = 1;j<baseWeapTypes[cWType].Count-1;j++)
                            {
                                cutArray[i] = cutArray[i]+$", {baseWeapTypes[cWType][j]}";
                            }
                            cutArray[i] = cutArray[i]+$" and {baseWeapTypes[cWType][baseWeapTypes[cWType].Count-1]}";
                        }
                    }
                    modified=true;
//                    Console.WriteLine(string.Join(' ',cutArray));
                }
            }

            return string.Join(' ',cutArray.Where(x => !string.IsNullOrEmpty(x)).ToArray());
        }

    }
}
