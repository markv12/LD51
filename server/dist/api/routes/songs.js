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
router.get('/some/:count', async (req, res) => {
    const count = parseInt(req.params.count);
    let randomSongs = [], bestSongs = [];
    randomSongs = await db_1.db.songs.getRandom(Math.ceil(count / 2));
    if (randomSongs.length < count)
        bestSongs = await db_1.db.songs.getBest(count - randomSongs.length);
    let allSongs = [...randomSongs, ...bestSongs];
    // remove just one of duplicate ids
    allSongs = allSongs.filter((song, i) => allSongs.findIndex((s) => s.id === song.id) === i);
    res.send(c.shuffleArray(allSongs));
    c.log(`Sent ${allSongs.length} general songs`);
});
router.post('/new', async (req, res) => {
    const song = req.body;
    if (!song?.name) {
        c.error('Invalid song uploaded (no name)', song);
        res.status(400).end();
        return;
    }
    for (const part of song?.parts) {
        const errors = c.validatePart(part);
        if (errors.length) {
            c.error('Invalid song part uploaded', part, errors);
            res.status(400).end();
            return;
        }
    }
    c.log('gray', 'Uploading new song', song);
    await db_1.db.songs.add(song);
    res.status(200).end();
});
router.get('/like/:id', async (req, res) => {
    const id = req.params.id;
    if (!id) {
        c.error('Missing song id to upvote', id);
        res.status(400).end();
        return;
    }
    const song = await db_1.db.songs.get(id);
    if (!song) {
        c.error('Invalid song id to upvote', id);
        res.status(400).end();
        return;
    }
    song.likes = song.likes || 0;
    song.likes++;
    song.ratio = song.likes / (song.dislikes || 1);
    song.recencyRatio = c.getRecencyRatio(song);
    await db_1.db.songs.update(song);
    res.status(200).end();
    c.log('gray', `Upvoted song ${id}, now has ${song.likes} likes and ${song.dislikes || 0} dislikes (recencyRatio ${c.r2(song.recencyRatio, 4)})`);
});
router.get('/dislike/:id', async (req, res) => {
    const id = req.params.id;
    if (!id) {
        c.error('Missing song id to downvote', id);
        res.status(400).end();
        return;
    }
    const song = await db_1.db.songs.get(id);
    if (!song) {
        c.error('Invalid song id to downvote', id);
        res.status(400).end();
        return;
    }
    song.dislikes = song.dislikes || 0;
    song.dislikes++;
    song.ratio = (song.likes || 0) / (song.dislikes || 1);
    song.recencyRatio = c.getRecencyRatio(song);
    await db_1.db.songs.update(song);
    res.status(200).end();
    c.log('gray', `Downvoted song ${id}, now has ${song.likes || 0} likes and ${song.dislikes} dislikes (ratio ${c.r2(song.recencyRatio, 4)})`);
});
exports.default = router;
//# sourceMappingURL=songs.js.map