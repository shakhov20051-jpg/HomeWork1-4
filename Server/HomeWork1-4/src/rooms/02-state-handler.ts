import { Room, Client } from "colyseus";
import { Schema, type, MapSchema } from "@colyseus/schema";

export class Player extends Schema {

    @type("number") speed = 0;

    @type("number") px = Math.floor(Math.random() * 30) - 15;
    @type("number") py = 0;
    @type("number") pz = Math.floor(Math.random() * 30) - 15;
    
    @type("number") vx = 0;
    @type("number") vy = 0;
    @type("number") vz = 0;    

    @type("number") rx = 0;    
    @type("number") ry = 0;    

    @type("boolean") fly = false;    

    @type("boolean") sq = false;    
}

export class State extends Schema {
    @type({ map: Player })
    players = new MapSchema<Player>();

    something = "This attribute won't be sent to the client-side";

    createPlayer(sessionId: string, data: any) {
        const player = new Player();
        player.speed = data.speed;
        this.players.set(sessionId, player);
    }

    removePlayer(sessionId: string) {
        this.players.delete(sessionId);
    }

    movePlayer (sessionId: string, data: any) {
        const player = this.players.get(sessionId);
        player.px = data.px;
        player.py = data.py;
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
            this.broadcast("Shoot", data, {except: client });
        });

        this.onMessage("move", (client, data) => {
           // console.log("StateHandlerRoom received message from", client.sessionId, ":", data);
            this.state.movePlayer(client.sessionId, data);
        });
    }

    onAuth(client, options, req) {
        return true;
    }

    onJoin (client: Client, data: any) {
        client.send("hello", "world");
        this.state.createPlayer(client.sessionId, data);
    }

    onLeave (client) {
        this.state.removePlayer(client.sessionId);
    }

    onDispose () {
        console.log("Dispose StateHandlerRoom");
    }

}
