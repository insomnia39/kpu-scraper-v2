const fs = require('fs');
const path = require('path');

function getTodayDate() {
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

function formatNumberPerThousand(number) {
    return number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

function formatNumberPercentage(number) {
    return `${number}%`;
}

function calculatePercentage(numerator, denominator) {
    return denominator === 0 ? 0 : Number(((numerator / denominator) * 100).toFixed(2));
}

function saveFile(data, title) {
    let jsonData = JSON.stringify(data)
    const filePath = path.join(path.dirname(title), path.basename(title));

    try {
        fs.mkdirSync(path.dirname(filePath), { recursive: true });
        fs.writeFileSync(title, jsonData);
        console.log(`JSON data written to ${title}`)
    } catch (e) {
        console.error("Error writing file:", e);
    }
}

function openJson(title) {
    const data = fs.readFileSync(title, 'utf8');
    return JSON.parse(data);
}

function generateVoteSummary() {
    const provinces = openJson(`./provinces.json`);
    for(let province of provinces) {
        const summary = {
            capres1: 0,
            capres2: 0,
            capres3: 0,
            suaraSah: 0,
            totalCapres: 0,
            different: 0,
        }
        const voteResults = openJson(`./result/${getTodayDate()}/${province.Nama}.json`);
        for(let voteResult of voteResults) {
            if(!voteResult.chart
                || !voteResult.administrasi
                || !voteResult.chart["100025"]
                || !voteResult.chart["100026"]
                || !voteResult.chart["100027"]) continue;
            summary.capres1 += voteResult.chart["100025"];
            summary.capres2 += voteResult.chart["100026"];
            summary.capres3 += voteResult.chart["100027"];
            summary.suaraSah += voteResult.administrasi.suara_sah;
        }
        summary.totalCapres = summary.capres1 + summary.capres2 + summary.capres3;
        summary.different = Math.abs(summary.totalCapres - summary.suaraSah);
        saveFile(summary, `./summary/${getTodayDate()}/${province.Nama}.json`);
    }
}

function getWinnerInRegion(summary) {
    const voteNumbers = [summary.capres1, summary.capres2, summary.capres3];
    const maxVoteNumber = Math.max(...voteNumbers);
    const nWinner = voteNumbers.filter(voteNumber => voteNumber === maxVoteNumber).length;
    return nWinner === 1 ? voteNumbers.indexOf(maxVoteNumber) + 1 : 0;
}

function readVoteSummary() {
    const provinces = openJson(`./provinces.json`);
    const summaries = []
    const summaryTotal = {
        capres1: 0,
        capres2: 0,
        capres3: 0,
        suaraSah: 0,
        totalCapres: 0,
        different: 0,
        province: "Total"
    }
    const summaryOrigin = {
        capres1: 0,
        capres2: 0,
        capres3: 0,
        suaraSah: 0,
        totalCapres: 0,
        different: 0,
        province: "Summary Origin"
    }
    const summaryAdjustment = {
        capres1: 0,
        capres2: 0,
        capres3: 0,
        suaraSah: 0,
        totalCapres: 0,
        different: 0,
        province: "Summary Adjustment"
    }

    const winningRegion = {
        capres1: 0,
        capres2: 0,
        capres3: 0,
        suaraSah: 0,
        totalCapres: 0,
        different: 0,
        province: "Winning Region"
    }

    summaries.push(summaryTotal);
    summaries.push(winningRegion);
    summaries.push(summaryOrigin);
    summaries.push(summaryAdjustment);

    let winnerRegions = [];
    for(let province of provinces) {
        const summary = openJson(`./summary/${getTodayDate()}/${province.Nama}.json`);
        summary.province = province.Nama;
        summaries.push(summary)
        summaryTotal.capres1 += summary.capres1;
        summaryTotal.capres2 += summary.capres2;
        summaryTotal.capres3 += summary.capres3;
        summaryTotal.suaraSah += summary.suaraSah;
        summaryTotal.totalCapres += summary.totalCapres;
        summaryTotal.different += summary.different;
        winnerRegions.push(getWinnerInRegion(summary));
    }

    summaryOrigin.capres1 = formatNumberPercentage(calculatePercentage(summaryTotal.capres1, summaryTotal.totalCapres));
    summaryOrigin.capres2 = formatNumberPercentage(calculatePercentage(summaryTotal.capres2, summaryTotal.totalCapres));
    summaryOrigin.capres3 = formatNumberPercentage(calculatePercentage(summaryTotal.capres3, summaryTotal.totalCapres));
    summaryOrigin.different = formatNumberPercentage(calculatePercentage(summaryTotal.different, summaryTotal.totalCapres));
    summaryOrigin.totalCapres = formatNumberPercentage(calculatePercentage(summaryTotal.totalCapres, summaryTotal.totalCapres));

    summaryAdjustment.capres1 = formatNumberPercentage(calculatePercentage(summaryTotal.capres1, summaryTotal.suaraSah));
    summaryAdjustment.capres2 = formatNumberPercentage(calculatePercentage(summaryTotal.capres2-summaryTotal.different, summaryTotal.suaraSah));
    summaryAdjustment.capres3 = formatNumberPercentage(calculatePercentage(summaryTotal.capres3, summaryTotal.suaraSah));
    summaryAdjustment.suaraSah = formatNumberPercentage(calculatePercentage(summaryTotal.suaraSah, summaryTotal.suaraSah));

    for(let winnerRegion of winnerRegions) {
        switch (winnerRegion) {
            case 1: winningRegion.capres1++; break;
            case 2: winningRegion.capres2++; break;
            case 3: winningRegion.capres3++; break;
        }
    }

    summaryTotal.capres1 = formatNumberPerThousand(summaryTotal.capres1);
    summaryTotal.capres2 = formatNumberPerThousand(summaryTotal.capres2);
    summaryTotal.capres3 = formatNumberPerThousand(summaryTotal.capres3);
    summaryTotal.suaraSah = formatNumberPerThousand(summaryTotal.suaraSah);
    summaryTotal.totalCapres = formatNumberPerThousand(summaryTotal.totalCapres);
    summaryTotal.different = formatNumberPerThousand(summaryTotal.different);

    for(let i = 4; i < summaries.length; i++) {
        let summary = summaries[i];
        summary.capres1 = formatNumberPerThousand(summary.capres1);
        summary.capres2 = formatNumberPerThousand(summary.capres2);
        summary.capres3 = formatNumberPerThousand(summary.capres3);
        summary.suaraSah = formatNumberPerThousand(summary.suaraSah);
        summary.totalCapres = formatNumberPerThousand(summary.totalCapres);
        summary.different = formatNumberPerThousand(summary.different);
    }

    console.table(summaries);
}

generateVoteSummary();
readVoteSummary();