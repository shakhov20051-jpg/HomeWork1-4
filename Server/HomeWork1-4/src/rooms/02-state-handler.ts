import { Room, Client } from "colyseus";
import { Schema, type, MapSchema } from "@colyseus/schema";
import { Console } from "console";

export class Player extends Schema {

    @type("number") speed = 0;
    @type("int8") hpMax = 0;
    @type("int8") hpCurrent = 0;

    @type("number") px = 0;
    @type("number") py = 0;
    @type("number") pz = 0;
    
    @type("number") vx = 0;
    @type("number") vy = 0;
    @type("number") vz = 0;    

    @type("number") rx = 0;    
    @type("number") ry = 0;    

    @type("boolean") fly = false;    

    @type("boolean") sq = false;    

    
    // @type("int8") scoreDie = 0; 
    // @type("int8") scoreWin = 0; 
    // @type("int8") numWeapon = 0; 
}

export class State extends Schema {
    @type({ map: Player })
    players = new MapSchema<Player>();

    
    
    something = "This attribute won't be sent to the client-side";
    
    createPlayer(sessionId: string, data: any) {
        
        const player = new Player();
        player.speed = data.speed;
        player.hpMax = data.hp;
        player.hpCurrent = data.hp;
        this.players.set(sessionId, player);
    }

    removePlayer(sessionId: string) {
        this.players.delete(sessionId);
    }

    movePlayer (sessionId: string, data: any) {
        const player = this.players.get(sessionId);
        player.py = data.py;
        player.px = data.px;
        player.pz = data.pz;
        player.vx = data.vx;
        player.vy = data.vy;
        player.vz = data.vz;
        player.rx = data.rx;
        player.ry = data.ry;
        player.fly = data.fly;
        player.sq = data.sq;
    }
}

export class StateHandlerRoom extends Room<State> {
    maxClients = 4;

    onCreate (options) {
        console.log("StateHandlerRoom created!", options);

        this.setPatchRate(100);

        this.setState(new State());

        
        this.onMessage("shoot", (client, data) => {     
            if(this.state.players.get(client.sessionId).hpCurrent > 0) 
            this.broadcast("Shoot", data.json, {except: client });
        });

        this.onMessage("move", (client, data) => {
            if(this.state.players.get(client.sessionId).hpCurrent > 0) 
            this.state.movePlayer(client.sessionId, data);
        });

        this.onMessage("revive", (client, data) => {
            const player = this.state.players.get(client.sessionId);
            player.hpCurrent = player.hpMax;
        });

        this.onMessage("damage", (client, data) => {
            const target = this.state.players.get(data.id); 
            target.hpCurrent -= data.value;

            if (target.hpCurrent <= 0) {
                target.hpCurrent = 0;

                const positions = [];
                this.state.players.forEach((p, id) => {
                    positions.push({
                        id,
                        px: p.px,
                        py: p.py,
                        pz: p.pz
                    });
                });

                const deadClient = this.clients.find(c => c.sessionId === data.id);
                if (deadClient)  deadClient.send("PlayersPositions", positions);
            }
        });

    }


    
    onAuth(client, options, req) {
        return true;
    }

    onJoin (client: Client, data: any) {

        this.state.createPlayer(client.sessionId, data);
    }

    onLeave (client) {
        this.state.removePlayer(client.sessionId);
    }

    onDispose () {
        console.log("Dispose StateHandlerRoom");
    }

}
