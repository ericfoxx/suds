using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace suds
{
    public class Room : IDescribable
    {
        //room intrinsics
        public string Name { get; set; }
        public int ID { get; set; }
        public Room North { get; set; }
        public Room East { get; set; }
        public Room South { get; set; }
        public Room West { get; set; }
        public Room Up { get; set; }
        public Room Down { get; set; }
        public Area ParentArea { get; set; }

        //room descriptions
        public string RoomDesc { get; set; }
        public bool Described { get; set; }
        //public string FloorDesc { get; set; }
        ///TODO: Add description types to Room class

        //room contents
        public Player player { get; set; }
        public List<IMob> mobs { get; set; }
        //public List<IItem> items { get; set; }
        public int gold { get; set; }
        ///TODO: Add more object types to rooms. Traps?

        public Room(string name)
        {
            Name = name;
            ID = suds.NextRoomID;
            suds.NextRoomID++;
            Described = false;
            ///TODO: Area = "Starting Area"; in Room ctor
        }

        public Room(string name, string desc)
            : this(name)
        {
            RoomDesc = desc;
        }

        public Room(string name, string desc, Room n, Room e, Room s, Room w, Room u, Room d)
            : this(name, desc)
        {
            if (n != null) LinkRooms(n, suds.Directions.North);
            if (s != null) LinkRooms(s, suds.Directions.South);
            if (e != null) LinkRooms(e, suds.Directions.East);
            if (w != null) LinkRooms(w, suds.Directions.West);
            if (u != null) LinkRooms(u, suds.Directions.Up);
            if (d != null) LinkRooms(d, suds.Directions.Down);
        }

        /// TODO: Make 'EnterRoom' method in Room

        public void Describe()
        {
            String.Format("==={0}===", Name).Color(suds.Fancy);
            RoomDesc.Color(suds.Normal);

            var exits = "Exits: ";
            exits += (North != null) ? "North " : "";
            exits += (South != null) ? "South " : "";
            exits += (East != null) ? "East " : "";
            exits += (West != null) ? "West " : "";
            exits += (Up != null) ? "Up " : "";
            exits += (Down != null) ? "Down " : "";
            exits.Color(suds.Normal);

            if (mobs != null) mobs.ForEach(m => m.Describe());

            ///TODO: Describe items in Room

            String.Format("There is {0} gold on the floor.", gold).Color(suds.Loot);

            if (!Described) Described = true;
        }

        

        public void LinkRooms(Room other, suds.Directions direction)
        {
            switch (direction)
            {
                case suds.Directions.North:
                    this.North = other;
                    other.South = this;
                    break;
                case suds.Directions.East:
                    this.East = other;
                    other.West = this;
                    break;
                case suds.Directions.South:
                    this.South = other;
                    other.North = this;
                    break;
                case suds.Directions.West:
                    this.West = other;
                    other.East = this;
                    break;
                case suds.Directions.Up:
                    this.Up = other;
                    other.Down = this;
                    break;
                case suds.Directions.Down:
                    this.Down = other;
                    other.Up = this;
                    break;
            }
        }
    }

    public class Encounter
    {
        public Player player { get; set; }
        public List<IMob> mobs { get; set; }

    }

    public class Area : IDescribable
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool IsLoaded { get; set; }
        public List<Room> Rooms { get; set; }
        public bool ContainsPlayer { get; set; }
        public Room CurrentRoom { get; set; }

        public Area(string name)
        {
            ID = suds.NextAreaID;
            suds.NextAreaID++;
            Name = name;

            IsLoaded = ContainsPlayer = false;
        }

        public void Travel(Room targetRoom)
        {
            CurrentRoom = targetRoom;
            CurrentRoom.Describe();
        }

        public void Describe()
        {
            ///TODO: Describe area
        }
    }
}
