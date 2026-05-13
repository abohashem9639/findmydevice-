import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const pkgPath = path.join(__dirname, "package.json");
const propsPath = path.join(__dirname, "Directory.Build.props");

if (!fs.existsSync(pkgPath) || !fs.existsSync(propsPath)) {
  console.error("Missing package.json or Directory.Build.props");
  process.exit(1);
}

let pkg;
try {
  pkg = JSON.parse(fs.readFileSync(pkgPath, "utf8"));
} catch (error) {
  console.error(`Failed to parse package.json: ${error.message}`);
  process.exit(1);
}
const version = pkg.version;

if (!version) {
  console.error("No 'version' field found in package.json");
  process.exit(1);
}

let props = fs.readFileSync(propsPath, "utf8");
const versionPattern = /<Version>[^<]*<\/Version>/;

if (versionPattern.test(props)) {
  props = props.replace(versionPattern, `<Version>${version}</Version>`);
} else {
  props = props.replace(/<\/PropertyGroup>/, `    <Version>${version}</Version>\n  </PropertyGroup>`);
}

fs.writeFileSync(propsPath, props);
console.log(`Synced Directory.Build.props to version ${version}`);
