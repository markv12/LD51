"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (k !== "default" && Object.prototype.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
    __setModuleDefault(result, mod);
    return result;
};
Object.defineProperty(exports, "__esModule", { value: true });
const c = __importStar(require("../../common"));
const express_1 = require("express");
const db_1 = require("../../db");
const router = (0, express_1.Router)();
router.get('/', (req, res) => {
    res.send('Hello Songs!');
});
router.get('/some/:count?/:scaleType?', async (req, res) => {
    const count = parseInt(req.params.count || '1') || 1;
    let scaleType = (req.params.scaleType || '').toLowerCase();
    if (!scaleType)
        scaleType = undefined;
    if (count === 1) {
        // * looking for initial partner, but we don't want an AWFUL partner.
        const howManyPartsToGet = 10;
        const randomParts = await db_1.db.parts.getRandom(howManyPartsToGet, scaleType);
        const best = randomParts.sort((a, b) => (b.ratio ?? -10000) - (a.ratio ?? -10000))[0];
        res.send([best]);
        c.log(`Sent ${[best].length} best part of ${howManyPartsToGet} random parts for scale type ${scaleType}`);
        return;
    }
    let randomParts = [], bestParts = [];
    // * likely to just send random parts — otherwise we end up with the same "best" parts every call
    const countToGetAsRandom = Math.floor(count / 2 + Math.random() * count * 0.75);
    randomParts = await db_1.db.parts.getRandom(countToGetAsRandom, scaleType);
    if (randomParts.length < count)
        bestParts = await db_1.db.parts.getBest(count - randomParts.length, scaleType);
    let allParts = [...randomParts, ...bestParts];
    // remove just one of duplicate ids
    allParts = allParts
        .filter((song, i) => allParts.findIndex((s) => s.id === song.id) === i, scaleType)
        .slice(0, count);
    res.send(c.shuffleArray(allParts));
    if (count > 1)
        allParts.forEach((p) => {
            db_1.db.parts.incrementGiven(p.id);
        });
    c.log(`Sent ${allParts.length} general part/s${scaleType ? ` of scale type ${scaleType}` : ``}`);
});
router.get('/chosen/:id', async (req, res) => {
    const id = req.params.id;
    if (!id) {
        c.error('Missing part id to increment chosen', id);
        res.status(400).end();
        return;
    }
    await db_1.db.parts.incrementChosen(id);
    res.status(200).end();
    // c.log(`Incremented chosen for part with id ${id}`)
});
router.post('/new', async (req, res) => {
    const part = req.body;
    if (!part?.name) {
        c.error('Invalid part uploaded: missing name');
        res.status(400).end();
        return;
    }
    const errors = c.validatePart(part);
    if (errors.length) {
        c.error('Invalid part uploaded', errors);
        res.status(400).end();
        return;
    }
    c.log('gray', 'Uploading new part', part.name);
    part.created = Date.now();
    await db_1.db.parts.add(part);
    res.status(200).send(part.id);
});
exports.default = router;
//# sourceMappingURL=parts.js.map