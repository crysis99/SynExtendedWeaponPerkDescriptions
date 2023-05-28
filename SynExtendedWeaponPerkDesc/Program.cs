using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;

namespace SynExtendedWeaponPerkDesc
{
    public class Program
    {   
        public class WeapType
        {
            public string Name;
            public FormKey baseFormKey;
            public string baseCat;
            public bool enabled;

            public WeapType (string name,string formKey)
            {
                this.Name = name;
                switch (formKey)
                {
                    case "06D932":
                        baseCat = "battleaxe";
                        break;
                    case "01E713":
                        baseCat = "dagger";
                        break;
                    case "06D931":
                        baseCat = "greatsword";
                        break;
                    case "01E714":
                        baseCat = "mace";
                        break;
                    case "01E711":
                        baseCat = "sword";
                        break;
                    case "01E712":
                        baseCat = "waraxe";
                        break;
                    case "06D930":
                        baseCat = "warhammer";
                        break;
                    default:
                        throw new ArgumentException();
                }
                this.baseFormKey = FormKey.Factory($"{formKey}:Skyrim.esm");
                this.enabled = false;
            }
        }
        // enum weapTypeEnum
        // {
        //     Claw,
        //     Scythe,
        //     Whip,
        //     Javelin,
        //     Maul,
        //     Club,
        //     Hatchet,
        //     Glaive,
        //     Trident,
        //     Rapier,
        //     Pike,
        //     Spear,
        //     Poleaxe,
        //     Halberd,
        //     Quarterstaff,
        //     Twinblade
        // }
        public static WeapType[] weapTypeArr = 
        {
            new WeapType("Claw","01E711"),
            new WeapType("Scythe","06D932"),
            new WeapType("Whip","01E711"),
            new WeapType("Javelin","01E711"),
            new WeapType("Maul","06D930"),
            new WeapType("Club","06D930"),
            new WeapType("Hatchet","01E712"),
            new WeapType("Glaive","06D931"),
            new WeapType("Trident","06D931"),
            new WeapType("Rapier","01E711"),
            new WeapType("Pike","06D931"),
            new WeapType("Spear","06D931"),
            new WeapType("Poleaxe","06D932"),
            new WeapType("Halberd","06D932"),
            new WeapType("Quarterstaff","06D930"),
            new WeapType("Twinblade","06D931")
        };
        //Going by animated heavy armoury basically halfpikes = pikes so not gonna have its own category
        /*
        0 = WeapTypeBattleaxe [KYWD:0006D932]
        1 = WeapTypeDagger [KYWD:0001E713]
        2 = WeapTypeGreatsword [KYWD:0006D931]
        3 = WeapTypeMace [KYWD:0001E714]
        4 = WeapTypeSword [KYWD:0001E711]
        5 = WeapTypeWarAxe [KYWD:0001E712]
        6 = WeapTypeWarhammer [KYWD:0006D930]
        */
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "SynExtendedWeaponPerkDesc.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var itemLink = new FormLink<IKeywordGetter>(FormKey.Factory("01E716:Skyrim.esm"));
            if (!itemLink.TryResolve<IKeywordGetter>(state.LinkCache, out var weapTypeStaffKeyw))
            {
                throw new ArgumentException();
            }

            foreach (IWeaponGetter weap in state.LoadOrder.PriorityOrder.OnlyEnabled().Weapon().WinningOverrides())
            {
                
                if(weap.Name == null){continue;}
                string weapName = weap.Name.String ?? "";
                if(weap.HasKeyword(weapTypeStaffKeyw)||weap.MajorFlags.HasFlag(Weapon.MajorFlag.NonPlayable)||(weap.EditorID ?? "").Contains("dummy",StringComparison.InvariantCultureIgnoreCase)||weapName.Trim() == ""||weap.Keywords==null){continue;}
                for(int l = 0; l<weapTypeArr.Count();l++)
                {
                    if(!weapTypeArr[l].enabled)
                    {
                        if(weapName.Contains(weapTypeArr[l].Name,StringComparison.InvariantCultureIgnoreCase)&&weap.HasKeyword(weapTypeArr[l].baseFormKey))
                        {
                            weapTypeArr[l].enabled=true;
                            Console.WriteLine($"Weapon Type: {weapTypeArr[l].Name} found");
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
            foreach(WeapType weapType in weapTypeArr)
            {
                if (!weapType.enabled){continue;}
                else if(weapType.Name=="Quarterstaff")
                {
                    baseWeapTypes[weapType.baseCat].Add("Quarterstaves");
                }
                else 
                {
                    baseWeapTypes[weapType.baseCat].Add(weapType.Name+"s");
                }
            }
            foreach(string list in baseWeapTypes.Keys)
            {
                Console.Write($"List Cat: {list}\n");
                foreach (string listkey in baseWeapTypes[list])
                {
                    Console.Write(listkey+",");
                }
                Console.Write("\n");
            }


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
                    Console.WriteLine($"{perk.FormKey}:{perk.Name}\nChanged Perk: {line}");
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
            modified = false;
            foreach (string cWType in baseWeapTypes.Keys)
            {
                if(((cWType!="sword"&&perkDesc.Contains(cWType,StringComparison.InvariantCultureIgnoreCase))||(cWType=="sword"&&perkDesc.Contains(cWType)&&!perkDesc.Contains("greatsword",StringComparison.InvariantCultureIgnoreCase)))&&baseWeapTypes[cWType].Count>1)
                {

                    for(int i = 0;i<cutArray.Count();i++)
                    {
                        if(((cWType!="sword"&&cutArray[i].Contains(cWType,StringComparison.InvariantCultureIgnoreCase))||(cWType=="sword"&&cutArray[i].Contains(cWType,StringComparison.InvariantCultureIgnoreCase)&&!cutArray[i].Contains("greatsword",StringComparison.InvariantCultureIgnoreCase))))
                        {
                            if(i>0)
                            {
                                if(cutArray[i-1]=="a"){cutArray[i-1]="";}
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
